using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCalParser
{
    internal interface ICalendarComponentParser
    {
        void Parse(Component component, IFieldSetReader reader);
    }

    internal class EventParser : ICalendarComponentParser
    {
        public void Parse(Component component, IFieldSetReader reader)
        {
            if(reader.Current != "BEGIN:VEVENT")
                throw new ApplicationException("No Event component detected");

            VCalendar cal = component as VCalendar;
            if (cal == null) return;

            VEvent ev = new VEvent();
            ev.Name = "VEVENT";
            ev.Fields = new List<ParsedField>();
            while (reader.Current != "END:VEVENT" && !reader.EndOfFeed())
            {
                if (reader.Current == "BEGIN:VEVENT")
                {
                    reader.MoveNext();
                    continue;
                }

                ParsedField field = reader.Current.ParseField(true);
                if(field != null)
                    ev.Fields.Add(field);

                

                reader.MoveNext();
            }
            cal.Events.Add(ev);
        }
    }
}
