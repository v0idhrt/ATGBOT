using System.Data.Common;

namespace telegram_bot_suka_blay;

public class User(int id, int age, int comradeId)
{

    public int Id { get; set; } = id;

    //public float Rate { get; set; }
    public int Age { get; set; } = age;
    public int ComradeId { get; set; } = comradeId;
}