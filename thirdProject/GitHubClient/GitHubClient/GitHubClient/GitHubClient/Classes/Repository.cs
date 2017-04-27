using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubClient.Classes
{
    class Repository
    {
        private string title;
        private User user;
        List<Commit> commits = new List<Commit>();

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }

        internal User User
        {
            get
            {
                return user;
            }

            set
            {
                user = value;
            }
        }

        internal List<Commit> Commits
        {
            get
            {
                return commits;
            }

            set
            {
                commits = value;
            }
        }
    }
}
