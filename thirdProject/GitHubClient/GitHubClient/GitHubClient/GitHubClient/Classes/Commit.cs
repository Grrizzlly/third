using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubClient.Classes
{
    class Commit
    {
        private string message;
        private DateTime date;
        private Repository repo;
        private string typeCommit;
        public string Message
        {
            get
            {
                return message;
            }

            set
            {
                message = value;
            }
        }

        public DateTime Date
        {
            get
            {
                return date;
            }

            set
            {
                date = value;
            }
        }

        internal Repository Repo
        {
            get
            {
                return repo;
            }

            set
            {
                repo = value;
            }
        }

        public string TypeCommit
        {
            get
            {
                return typeCommit;
            }

            set
            {
                typeCommit = value;
            }
        }

        
    }
}
