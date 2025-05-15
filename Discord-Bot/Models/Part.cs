namespace Discord_Bot.Models
{
    public class Part : SchedulePart
    {
        public List<ulong> ActiveUsers { get; set; }

        public Part(
            string name,
            ulong leadId,
            DateTime date,
            ulong activeUserId1 = 0,
            ulong activeUserId2 = 0,
            ulong activeUserId3 = 0) : 
                base(name, leadId, date)
        {
            ActiveUsers = [];

            if (activeUserId1 != 0)
                ActiveUsers.Add(activeUserId1);

            if (activeUserId2 != 0)
                ActiveUsers.Add(activeUserId2);

            if (activeUserId3 != 0)
                ActiveUsers.Add(activeUserId3);
        }

        public Part() 
        {
            ActiveUsers = new List<ulong>();
        }
    }
}
