using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCalToolKit
{
    internal class DefaultComponentProcessor
    {
        public Component Parse(IFieldSetReader reader)
        {
           return Parse(reader, null);
        }
        public Component Parse(IFieldSetReader reader,Component parent)
        {
            if (!reader.Current.IsComponent())
                throw new ApplicationException("No component detected");

            Component comp = new Component();
            comp.Name = reader.Current.GetComponentName();
            while (!reader.Current.StartsWith("END:" + comp.Name) && !reader.EndOfFeed())
            {

                if (reader.Current.IsComponent() && !reader.Current.StartsWith( "BEGIN:" + comp.Name))
                    Parse(reader, comp);

                ParsedField field = reader.Current.ParseField(true);
                if (field != null)
                {
                    if(comp.Fields == null)comp.Fields = new List<ParsedField>(); 
                     comp.Fields.Add(field);
                }
                   
                if (parent != null)
                {
                    if (parent.AdditionalComponents == null)
                        parent.AdditionalComponents = new List<Component>();

                    parent.AdditionalComponents.Add(comp);
                }
                    


                reader.MoveNext();
            }

            return comp.Fields == null ? null : comp;

        }
    }
}
