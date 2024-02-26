namespace ED.Atlas.Svc.TC.Ice.FE.Messages
{
    public class NoPartyIDs : IntField
    {
        public NoPartyIDs(int fixId) : base(fixId)
        {
        }

        public NoPartyIDs(string value, int fixId) : base(value, fixId)
        {
        }

     
    }
}