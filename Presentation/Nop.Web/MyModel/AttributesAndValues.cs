using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.MyModel
{
    
    public class ValueList
    {
        public string value { get; set; }
    }

    public class AttributeNameList
    {
        public string attributeName { get; set; }
        public IList<ValueList> valueList { get; set; }
    }

    public class AttributesAndValues
    {
        public int itemId { get; set; }
        public IList<AttributeNameList> attributeNameList { get; set; }
    }
}
