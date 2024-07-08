namespace telegram_bot_suka_blay
{
    using Npgsql;

    // public class ApplicationContext : DbContext
    // {
    //     public DbSet<User> Users { get; set; } = null!;
    //
    //     public ApplicationContext()
    //     {
    //         Database.EnsureCreated();
    //     }
    //
    //     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //     {
    //         optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=users;Username=postgres;Password=root");
    //     }
    // }

    public class DataBase
    {
        static NpgsqlConnection con;
        public DataBase()
        {
           con = new NpgsqlConnection(
                connectionString: "Server=localhost;Port=5432;Username=postgres;Password=root;Database=users");
           con.Open();
        }
        
        public static async void Test()
        {
            con = new NpgsqlConnection(
                connectionString: "Server=localhost;Port=5432;Username=postgres;Password=root;Database=users");
            con.Open();
            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;

            await GetBySubject();
        }
        
        
        public static async Task<IEnumerable<User>> GetBySubject()
        {
            con = new NpgsqlConnection(
                connectionString: "Server=localhost;Port=5432;Username=postgres;Password=root;Database=users");
            con.Open();
            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;
            cmd.CommandText = "SELECT \"Id\", \"Age\", \"ComradeId\" FROM users.users;";
            NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            var result = new List<User>();
            while (await reader.ReadAsync())
            {
                Console.WriteLine(reader["Id"]);
                Console.WriteLine(reader["Age"]);
                Console.WriteLine(reader["ComradeId"]);
                result.Add(new User(
                    id: (int)reader["Id"],
                    age: (int)reader["Age"],
                    comradeId: (int)reader["ComradeId"]));
            }
            return result;
        }
        
    }
    // public class DataBase
    // {
    //     public static void GetData()
    //     {
    //         using (ApplicationContext db = new ApplicationContext())
    //         {
    //             // создаем два объекта User
    //             User user1 = new User { Id = 123213123, Age = 33 };
    //             User user2 = new User { Id = 77777777, Age = 26 };
    //
    //             // добавляем их в бд
    //             db.Users.AddRange(user1, user2);
    //             db.SaveChanges();
    //         }
    //
    //         using (ApplicationContext db = new ApplicationContext())
    //         {
    //             // получаем объекты из бд и выводим на консоль
    //             var users = db.Users.ToList();
    //             Console.WriteLine("Users list:");
    //             foreach (User u in users)
    //             {
    //                 Console.WriteLine($"{u.Id} - {u.Age}");
    //             }
    //         }
    //     }
    // };
}

