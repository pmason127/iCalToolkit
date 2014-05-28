using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCalToolKit
{
   public  class VCalendar:Component
    {
       public VCalendar(Component comp):base(comp)
       {
            
       }

       public VCalendar()
       {
           
       }
       private IList<VEvent> _events = null; 
       public IList<VEvent> Events
       {
           get {
               return _events ??
                      (_events = AdditionalComponents.Where(c => c.Name == "VEVENT").Select(c => new VEvent(c)).ToList());
           }
       }

    }

    public class DateTimeInfo
    {
        public DateTimeInfo(DateTime utcTime,TimeZone tz)
        {
            this.UtcDateTime = utcTime;
            this.TimeZone = tz;

            this.OffsetTime = tz.ToLocalTime(this.UtcDateTime);

            this.IsDaylightSavingsTime = tz.IsDaylightSavingTime(this.OffsetTime);
            if (IsDaylightSavingsTime)
            {
                this.TimeZoneName = tz.DaylightName;
                this.IsDaylightSavingsTime = true;
            }
            else
            {
                this.TimeZoneName = tz.StandardName;
            }
        }
        public DateTime UtcDateTime { get; private set; }
        public DateTime OffsetTime { get; private set; }
        public TimeZone TimeZone { get; private set; }
        public bool IsDaylightSavingsTime { get; private set; }
        public string TimeZoneName { get; private set; }
    }

   public class VEvent:Component
   {
       public VEvent(Component comp):base(comp)
       {
            
       }
       public  DateTimeInfo DTSTART
       {
           get
           {
               var field = GetFieldData("DTSTART");
               return field.ParseICSDateTime();
           }
       }
       public string UID { get { return GetFieldData("UID"); } }
   }


    public class Attendee
    {
        public string Name { get; set; }
        public string MailTo { get; set; }
    }

    public class Component
    {
        public Component()
        {
            this.AdditionalComponents = new List<Component>();
            this.Fields = new List<ParsedField>();
        }
        public Component(Component component)
        {
            this.Name = component.Name;
            this.Fields = component.Fields;
            this.AdditionalComponents = component.AdditionalComponents;
        }
        public string Name { get; set; }
        public IList<ParsedField> Fields { get; set; }
        public IList<Component> AdditionalComponents { get; set; }

        public string GetFieldData(string name)
        {
            var field = Fields.Where(f => f.FieldName == name).FirstOrDefault();
            if (field != null)
            {
                return field.FieldValue;
            }
            return null;
        }
        public Component GetComponent(string name)
        {
            if (AdditionalComponents == null) return null;
            var comp = AdditionalComponents.Where(f => f.Name == name).FirstOrDefault();
            return comp;
        }
    }

    public class ParsedField
    {
        public ParsedField()
        {
            AdditionalInformation = new Dictionary<string, string>();
        }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public IDictionary<string, string> AdditionalInformation { get; private set; }
    }
}
