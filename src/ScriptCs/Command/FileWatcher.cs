using System;
using System.Threading;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    public sealed class FileWatcher : IDisposable
    {
        private readonly object _timerLock = new object();
        private readonly string _file;
        private readonly int _intervalMilliseconds;
        private readonly IFileSystem _fileSystem;

        private DateTime _lastWriteTime;
        private Timer _timer;

        public FileWatcher(string file, int intervalMilliseconds, IFileSystem fileSystem)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);

            _file = file;
            _intervalMilliseconds = intervalMilliseconds;
            _fileSystem = fileSystem;
        }

        public event EventHandler Changed;

        public void Start()
        {
            if (_timer != null)
            {
                return;
            }

            _lastWriteTime = _fileSystem.GetLastWriteTime(_file);
            _timer = new Timer(_ => CheckLastWriteTime(), null, Timeout.Infinite, Timeout.Infinite);
            _timer.Change(_intervalMilliseconds, Timeout.Infinite);
        }

        public void Stop()
        {
            if (_timer == null)
            {
                return;
            }

            lock (_timerLock)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private void CheckLastWriteTime()
        {
            lock (_timerLock)
            {
                if (_timer == null)
                {
                    return;
                }

                var previousLastWriteTime = _lastWriteTime;
                _lastWriteTime = _fileSystem.GetLastWriteTime(_file);
                if (_lastWriteTime != previousLastWriteTime)
                {
                    var changed = this.Changed;
                    if (changed != null)
                    {
                        changed.Invoke(this, EventArgs.Empty);
                    }
                }

                _timer.Change(_intervalMilliseconds, Timeout.Infinite);
            }
        }
    }
}
