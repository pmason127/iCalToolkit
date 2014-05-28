using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCalToolKit
{
    public class RFC5545Parser
    {
        private static readonly string BeginCalendar = "BEGIN:VCALENDAR";
        private static readonly string EndCalendar = "END:VCALENDAR";

        public RFC5545Parser()
        {
        }
        public VCalendar Parse(Stream stream)
        {
            StringBuilder sb;
            string data;
            using (StreamReader rdr = new StreamReader(stream))
            {
               data = rdr.ReadToEnd();
            }

            IList<string> unwrappedFields = null;
            if(!string.IsNullOrEmpty(data))
                unwrappedFields = UnWrap(data);

          
            FieldSetReader reader = new FieldSetReader(unwrappedFields.ToArray());
            var calendar =Parse(reader,null);

            return new VCalendar(calendar);
        }
        private Component  Parse(IFieldSetReader reader, Component parent)
        {
            var name = reader.Current.GetComponentName();
            var compStart = string.Concat("BEGIN:", name);
            var compEnd = string.Concat("END:", name);

            Component newComp = new Component();
            newComp.Name = name;
            while (reader.Current != compEnd && !reader.EndOfFeed())
            {
               
                if (reader.Current.IsComponent() && reader.Current != compStart)
                {
                    Parse(reader, newComp);
                }
                   

                ParsedField field = reader.Current.ParseField(true);
                if (field != null)
                {
                    if (newComp.Fields == null) newComp.Fields = new List<ParsedField>();
                    newComp.Fields.Add(field);
                }

                reader.MoveNext();
            }
            if (parent != null)
            {
                if (parent.AdditionalComponents == null)
                    parent.AdditionalComponents = new List<Component>();

                parent.AdditionalComponents.Add(newComp);
            }

            return newComp;
        }
        private IList<string> UnWrap(string data)
        {
      
            //initial split, from this we can process each line for wraps
            string[] entries = data.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

            IList<string> unwrappedEntries = new List<string>();
            string row = "";
            foreach (string str in entries)
            {
                //Spec states that a line continuation will start with a tab or single space
                if (!str.StartsWith("\t") && !str.StartsWith(" "))
                {
                    //We have a new data record so commit any data we had
                    if(!string.IsNullOrEmpty(row))
                        unwrappedEntries.Add(row);

                    //start a new row
                    row = str;
                }
                else
                {
                   //a tab signifies a continuation so we remove it and append it to the previous
                    row = string.Concat(row, str.Replace("\t", str.Remove(0, 1)));
                }
            }

            //commit any remaining data
            if (!string.IsNullOrEmpty(row))
                unwrappedEntries.Add(row);

            return unwrappedEntries;
        }
    }

    public interface IFieldSetReader
    {
        bool MoveNext();
        bool MovePrevious();
        bool EndOfFeed();
        void MoveToStart();
        void Position(int position);
        string Current { get; }
    }

    public class  FieldSetReader : IFieldSetReader
    {
        private string[] _fields;
        private int _position = 0;
        public FieldSetReader(string[] fields)
        {
            if(_fields == null)
                _fields = new string[0];

            _fields = fields;
            _position = 0;
        }

        public bool MoveNext()
        {
            _position++;
          if (_position >= _fields.Length)
          {
              _position = _fields.Length - 1;
              return false;
          }
            return true;
        }
        public bool MovePrevious()
        {
            _position--;
            if (_position < 0)
            {
                return false;
                _position = 0;
            }
            return true;
        }
        public bool EndOfFeed()
        {
            return (_position + 1) >= _fields.Length;
        }
        public void MoveToStart()
        {
            _position = 0;
        }
        public void Position(int position)
        {
            if (position > _fields.Length - 1)
                _position = _fields.Length - 1;
            else if (position < 0)
                _position = 0;
            else
                _position = position;
            
        }
        public string Current
        {
            get { return _fields[_position]; }
        }
    }
}
