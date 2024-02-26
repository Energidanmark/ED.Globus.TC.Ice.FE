namespace ED.Atlas.Svc.TC.Ice.FE.Trades
{
    public class PropertyMapper
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public PropertyMapper(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}