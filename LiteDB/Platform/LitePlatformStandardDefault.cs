using System;
using System.Threading.Tasks;

namespace LiteDB.Platform
{
    public class LitePlatformStandardDefault : ILitePlatform
    {
        private readonly LazyLoad<IFileHandler> _fileHandler;
        private readonly LazyLoad<IReflectionHandler> _reflectionHandler = new LazyLoad<IReflectionHandler>(() => new ExpressionReflectionHandler());

        public LitePlatformStandardDefault() : this(".") { }

        public LitePlatformStandardDefault(String path)
        {
            _fileHandler = new LazyLoad<IFileHandler>(() => new FileHandlerSyncOverAsync(path));
        }

        public IFileHandler FileHandler
        {
            get
            {
                return _fileHandler.Value;
            }
        }

        public IReflectionHandler ReflectionHandler
        {
            get
            {
                return _reflectionHandler.Value;
            }
        }

        public IEncryption GetEncryption(string password)
        {
            return new RijndaelEncryption(password);
        }

        public void WaitFor(int milliseconds)
        {
            Task.Delay(milliseconds).Wait(milliseconds);
        }
    }
}
