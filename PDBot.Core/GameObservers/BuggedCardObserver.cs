using PDBot.Core.Data;
using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.GameObservers
{
    class BuggedCardObserver : IGameObserver
    {
        private readonly List<string> warnings = new List<string>();

        public bool PreventReboot => false;

        public Task<IGameObserver> GetInstanceForMatchAsync(IMatch match)
        {
            return Task.FromResult<IGameObserver>(new BuggedCardObserver());
        }

        public string HandleLine(GameLogLine gameLogLine)
        {
            foreach (var name in gameLogLine.Cards)
            {
                if (warnings.Contains(name))
                    continue;
                if (API.BuggedCards.IsCardBugged(name) is API.BuggedCards.Bug bug)
                {
                    warnings.Add(name);
                    var description = bug.Description.Replace("[", "").Replace("]","");
                    return $"[sU]{name}[sU] has a {bug.Classification} bug.  {description}";
                }
            }
            return null;
        }

        public void ProcessWinner(string winner, int gameID)
        {

        }

        public bool ShouldJoin(IMatch match)
        {
            // Bugged cards is never enough of a reason to join a match.  It's just an added bonus
            return false;
        }
    }
}
