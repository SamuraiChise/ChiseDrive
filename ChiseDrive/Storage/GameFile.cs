using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

namespace ChiseDrive.Storage
{
    public abstract class GameFile
    {
        string title = null;
        string name = null;

        protected string Filename { get { return name; } }

        protected static StorageDevice device = null;

        bool IsConnected { get { return device != null && device.IsConnected; } }
        bool pendingRead = false;
        bool pendingWrite = false;
        bool pendingDelete = false;

        StorageContainer container = null;
        FileStream file = null;

        protected FileStream Data
        {
            get
            {
                return file;
            }
            set
            {
                file = value;
            }
        }

        public GameFile(string title, string filename)
        {
            this.title = title;
            this.name = filename;
        }

        public void TrySelectDevice()
        {
            if (!Guide.IsVisible)
            {
                try
                {
                    Guide.BeginShowStorageDeviceSelector(SelectCallback, null);
                }
                catch
                {
                }
            }
        }

        public void SelectCallback(IAsyncResult result)
        {
            if (result != null
                && result.IsCompleted)
            {
                try
                {
                    device = Guide.EndShowStorageDeviceSelector(result);
                }
                catch
                {
                }
                finally
                {
                }

                if (IsConnected)
                {
                    if (pendingRead)
                    {
                        DoRead();
                    }
                    if (pendingWrite)
                    {
                        DoWrite();
                    }
                    if (pendingDelete)
                    {
                        DoDelete();
                    }
                }
            }
        }

        void ClearPending()
        {
            pendingRead = false;
            pendingWrite = false;
            pendingDelete = false;
        }

        public void TryRead()
        {
            ClearPending();
            if (!IsConnected)
            {
                pendingRead = true;
                TrySelectDevice();
            }
            else
            {
                DoRead();
            }
        }

        public void TryWrite()
        {
            ClearPending();
            if (!IsConnected)
            {
                pendingWrite = true;
                TrySelectDevice();
            }
            else
            {
                DoWrite();
            }
        }

        public void TryDelete()
        {
            ClearPending();
            if (!IsConnected)
            {
                pendingDelete = true;
                TrySelectDevice();
            }
            else
            {
                DoDelete();
            }
        }

        void DoRead()
        {
            try
            {
                OpenRead();
            }
            catch
            {
                if (container != null)
                {
                    container.Dispose();
                    container = null;
                }
                else if (device != null
                        && device.IsConnected)
                {
                    device.DeleteContainer(title);
                }
            }
            if (file != null)
            {
                Read();
            }
            try
            {
                Close();
            }
            catch
            {
                if (container != null)
                {
                    container.Dispose();
                    container = null;
                }
            }
        }

        void DoWrite()
        {
            try
            {
                OpenWrite();
            }
            catch (Exception e)
            {
                if (container != null)
                {
                    container.Dispose();
                    container = null;
                }
                else if (device != null
                    && device.IsConnected)
                {
                    device.DeleteContainer(title);
                }
            }
            if (file != null)
            {
                Write();
            }
            try
            {
                Close();
            }
            catch
            {
                if (container != null)
                {
                    container.Dispose();
                    container = null;
                }
            }
        }

        void DoDelete()
        {
            try
            {
                container = device.OpenContainer(title);
                string filename = Path.Combine(container.Path, name);
                    
                File.Delete(filename);
            }
            catch
            {
                if (container != null)
                {
                    container.Dispose();
                    container = null;
                }
            }
        }

        protected void OpenRead()
        {
            container = device.OpenContainer(title);
            string filename = Path.Combine(container.Path, name);

            file = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read);
        }

        protected void OpenWrite()
        {
            container = device.OpenContainer(title);
            string filename = Path.Combine(container.Path, name);

            file = File.Open(filename, FileMode.Create, FileAccess.Write);
        }

        protected abstract void Read();
        protected abstract void Write();

        protected void Close()
        {
            if (file != null)
            {
                file.Close();
                file = null;
            }

            if (container != null)
            {
                container.Dispose();
                container = null;
            }

            ClearPending();
        }
    }
}
