using System.Data.Common;

namespace telegram_bot_suka_blay;

public class User
{

    public long Id { get; set; }

    //public float Rate { get; set; }
    public int Age { get; set; }
    public long ComradeId { get; set; }
    public string State { get; set; }
    public User()
    {
        Id = 0;
        Age = 0;
        ComradeId = 0;
        State = "START";
    }
    public User(long id)
    {
        Id = id;
        Age = 0;
        ComradeId = 0;
        State = "START";
    }
    public User(long id, int age, long comradeId, string state)
    {
        Id = id;
        Age = age;
        ComradeId = comradeId;
        State = state;
    }
}
