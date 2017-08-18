using System;
using System.IO;
using System.Linq;

namespace LiteDB
{
    public partial class LiteEngine
    {
        /// <summary>
        /// Reduce disk size re-arranging unused spaces. Can change password. If temporary disk was not provided, use MemoryStream temp disk
        /// </summary>
        public long Shrink(string password = null, IDiskService temp = null)
        {
            var originalSize = _disk.FileLength;

            // if temp disk are not passed, use memory stream disk
            temp = temp ?? new StreamDiskService(new MemoryStream());

            using (_locker.Reserved())
            using (_locker.Exclusive())
            using (var engine = new LiteEngine(temp, password))
            {
                // read all collection
                foreach (var col in _collections.GetAll())
                {
                    var collectionName = col.CollectionName;

                    // first create all user indexes (exclude _id index)
                    foreach (var index in col.GetIndexes(false))
                    {
                        engine.EnsureIndex(collectionName, index.Field, index.Unique);
                    }

                    // now copy documents 
                    var docs = this.FindAll(collectionName);

                    engine.InsertBulk(collectionName, docs);
                }

                // copy user version
                engine.UserVersion = this.UserVersion;

                // set current disk size to exact new disk usage
                _disk.SetLength(temp.FileLength);

                // read new header page to start copy
                var header = (HeaderPage)BasePage.ReadPage(temp.ReadPage(0));

                // copy (as is) all pages from temp disk to original disk
                for (uint i = 0; i <= header.LastPageID; i++)
                {
                    var page = temp.ReadPage(i);

                    _disk.WritePage(i, page);
                }

                // create/destroy crypto class
                _crypto = password == null ? null : new AesEncryption(password, header.Salt);

                // initialize all services again (crypto can be changed)
                this.InitializeServices();
                
                // return how many bytes are reduced
                return originalSize - temp.FileLength;
            }
        }
    }
}
