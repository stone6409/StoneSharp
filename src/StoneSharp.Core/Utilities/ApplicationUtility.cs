using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StoneSharp.Core.Utilities
{
    public static class ApplicationUtility
    {
        private static string _applicationName;

        public static string ApplicationName
        {
            get
            {
                if (_applicationName == null)
                {
                    // 获取当前进程的可执行文件路径
                    string exePath = System.Reflection.Assembly.GetEntryAssembly()?.Location;

                    if (!string.IsNullOrEmpty(exePath))
                    {
                        _applicationName = Path.GetFileNameWithoutExtension(exePath);
                    }
                    else
                    {
                        _applicationName = AppDomain.CurrentDomain.FriendlyName;
                    }
                }

                return _applicationName;
            }
            set
            {
                _applicationName = value;
            }
        }
    }
}