using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommonStyleLib.Models;
using Reactive.Bindings;

namespace CookInformationViewer.Models.Searchers
{
    public class SearchNode
    {
        public SearchNode? Parent { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<SearchNode>? Children { get; set; }
    }

    public class SearchWindowModel : ModelBase
    {
        private ObservableCollection<SearchNode> _searchNodes = new();

        public ObservableCollection<SearchNode> SearchItems
        {
            get => _searchNodes;
            set => SetProperty(ref _searchNodes, value);
        }

        public SearchWindowModel()
        {
            SearchItems.Add(new SearchNode
            {
                Name = "1",
                Children = new List<SearchNode>
                {
                    new SearchNode
                    {
                        Name = "2"
                    },
                    new SearchNode
                    {
                        Name = "3"
                    }
                }
            });
        }
    }
}
