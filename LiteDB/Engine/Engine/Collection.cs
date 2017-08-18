﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDB
{
    public partial class LiteEngine
    {
        /// <summary>
        /// Returns all collection inside datafile
        /// </summary>
        public IEnumerable<string> GetCollectionNames()
        {
            using (_locker.Shared()) {
                var header = _pager.GetPage<HeaderPage>(0);

                return header.CollectionPages.Keys.ToArray(); // make a copy so enumerating won't happen outside of the lock.
            }
        }

        /// <summary>
        /// Checks if a collection exists on database. Collection name is case insensitive
        /// </summary>
        public bool CollectionExists(string name)
        {
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException("name");

            using (_locker.Shared()) {
                var header = _pager.GetPage<HeaderPage>(0);

                return header.CollectionPages.ContainsKey(name);
            }
        }

        /// <summary>
        /// Drop collection including all documents, indexes and extended pages
        /// </summary>
        public bool DropCollection(string collection)
        {
            if (collection.IsNullOrWhiteSpace()) throw new ArgumentNullException("collection");

            return this.Transaction<bool>(collection, false, (col) => {
                if (col == null) return false;

                _log.Write(Logger.COMMAND, "drop collection {0}", collection);

                _collections.Drop(col);

                return true;
            });
        }

        /// <summary>
        /// Rename a collection
        /// </summary>
        public bool RenameCollection(string collection, string newName)
        {
            if (collection.IsNullOrWhiteSpace()) throw new ArgumentNullException("collection");
            if (newName.IsNullOrWhiteSpace()) throw new ArgumentNullException("newName");

            return this.Transaction<bool>(collection, false, (col) => {
                if (col == null) return false;

                _log.Write(Logger.COMMAND, "rename collection '{0}' -> '{1}'", collection, newName);

                // check if newName already exists
                if (this.CollectionExists(newName)) {
                    throw LiteException.AlreadyExistsCollectionName(newName);
                }

                // change collection name and save
                col.CollectionName = newName;

                // set collection page as dirty
                _pager.SetDirty(col);

                // update header collection reference
                var header = _pager.GetPage<HeaderPage>(0);

                header.CollectionPages.Remove(collection);
                header.CollectionPages.Add(newName, col.PageID);

                _pager.SetDirty(header);

                return true;
            });
        }
    }
}