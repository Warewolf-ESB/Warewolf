using System.Text.RegularExpressions;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Diagnostics
{


    public class DirectoryPath : PropertyChangedBase, IDirectoryPath
    {
        private string _path;
        private string _pathToSerialize;

        public string RealPath
        {
            get
            {
                return _path;
            }
            set
            {
                if (_path == value)
                {
                    return;
                }

                _path = value;
                SetSerializePath();
                NotifyOfPropertyChange(() => RealPath);
            }
        }

        public string PathToSerialize
        {
            get
            {
                return _pathToSerialize;
            }
            set
            {
                if (_pathToSerialize == value)
                {
                    return;
                }

                _pathToSerialize = value;
                SetRealPath();
                NotifyOfPropertyChange(() => PathToSerialize);
            }
        }

        public void SetRealPath()
        {
            RealPath = Regex.Replace(PathToSerialize, "/", "\\");
        }

        public void SetSerializePath()
        {
            PathToSerialize = Regex.Replace(RealPath, "\\\\", "/");
        }
    }
}