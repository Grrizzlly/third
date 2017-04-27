using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubClient.Classes
{
    class User
    {
        private string userName;
        private List<Repository> repos = new List<Repository>();
        private string urImage;

        public string UserName
        {
            get
            {
                return userName;
            }

            set
            {
                userName = value;
            }
        }

        internal List<Repository> Repos
        {
            get
            {
                return repos;
            }

            set
            {
                repos = value;
            }
        }

        public string UrImage
        {
            get
            {
                return urImage;
            }

            set
            {
                urImage = value;
            }
        }
    }
}
