using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetaTesterSite.Models.Shared
{
    public class DataTablesAjaxPostModel
    {
        public int draw { get; set; }
        public int? start { get; set; }
        public int? length { get; set; }
        public Search search { get; set; }

        public DataTablesAjaxPostModel()
        {
            this.search = new Search();
        }
    }
    
    public class Search
    {
        public string value { get; set; }
        public string regex { get; set; }
    }
}
