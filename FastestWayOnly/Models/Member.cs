using System.Diagnostics;

namespace FastestWayOnly.Models
{
    public class Member
    {
        const string MembersPath = "Resources/members.csv";
        const string ExMembersPath = "Resources/exmembers.csv";
        public string Name { get; set; }
        public DateOnly Date { get; set; }
        public string RecruitMap { get; set; }
        public string RecruitVideo { get; set; }
        public string Nationality { get; set; }
        public string YouTube { get; set; }
        public string Twitch { get; set; }
        static Member[] members_ = new Member[0];
        static Member[] exMembers_ = new Member[0];
        static ulong[] logins_ = new ulong[0];
        public Member(string name, DateOnly date, string recruitMap, string recruitVideo, string nationality, string youTube, string twitch)
        {
            Name = name;
            Date = date;
            RecruitMap = recruitMap;
            RecruitVideo = recruitVideo;
            Nationality = nationality;
            YouTube = youTube;
            Twitch = twitch;
        }
        public static Member[] GetMembers()
        {
            if (members_.Length == 0)
            {
                LoadData();
                Debug.WriteLine(members_.Length + " FWO members loaded");
            }
            return members_;
        }

        public static Member[] GetExMembers()
        {
            if (members_.Length == 0)
            {
                LoadData();
                Debug.WriteLine(members_.Length + " ex FWO members loaded");
            }
            return exMembers_;
        }

        public static ulong[] GetLogins()
        {
            if (members_.Length == 0)
            {
                LoadData();
                Debug.WriteLine(members_.Length + " FWO members loaded");
            }
            return logins_;
        }

        static void LoadDataTo(string fileName, ref Member[] members, bool listLogins)
        {
            List<Member> data = new List<Member>();
            List<ulong> logins = new List<ulong>();
            using (StreamReader reader = new StreamReader(fileName))
            {
                // Skip header line
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (line != null)
                    {
                        string[] values = line.Split(';');
                        string[] dateValues = values[1].Split('/');
                        int day = int.Parse(dateValues[0]);
                        int month = int.Parse(dateValues[1]);
                        int year = int.Parse(dateValues[2]);
                        DateOnly date = new DateOnly(year, month, day);
                        data.Add(new Member(values[0], date, values[2], values[3], values[4], values[5], values[6]));

                        if (listLogins)
                        {
                            for (int i = 7; i < 11; i++)
                            {
                                if (!string.IsNullOrEmpty(values[i]))
                                {
                                    try
                                    {
                                        ulong id = ulong.Parse(values[i]);
                                        logins.Add(id);
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (listLogins)
                logins_ = logins.ToArray();

            members = data.ToArray();
        }

        static void LoadData()
        {
            LoadDataTo(MembersPath, ref members_, true);
            LoadDataTo(ExMembersPath, ref exMembers_, false);
        }
    }
}
