namespace ED.Atlas.Svc.TC.Ice.FE.Messages
{
    public abstract class IntField
    {
        protected string Value;
        protected  int FixId;
        
        protected IntField(int fixId)
        {
            FixId = fixId;
        }
        protected IntField(string value, int fixId)
        {
            Value = value;
            FixId = fixId;
        }

        public IntField SetValue(string value)
        {
            Value = value;
            return this;
        }
    }
    
}