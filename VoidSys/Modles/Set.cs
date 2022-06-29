using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class Set:DbContext
    {
        public Set(DbContextOptions<Set> options) : base(options) { }

        public virtual DbSet<v_Area> v_area { get; set; }

        public virtual DbSet<v_Users> v_users { get; set; }

        public virtual DbSet<v_UsersData> v_usersData { get; set; }

        public virtual DbSet<v_Page> v_page { get; set; }

        public virtual DbSet<v_Tag> v_tag { get; set; }

        public virtual DbSet<v_Admin> v_admins { get; set; }

        public virtual DbSet<v_Roll> v_roll { get; set; }

        public virtual DbSet<v_Say> v_say { get; set; }

        public virtual DbSet<v_Resay> v_resay { get; set; }

        public virtual DbSet<v_DayHeight> v_dayheight { get; set; }

        public virtual DbSet<History> history { get; set; }

        public virtual DbSet<Save> save { get; set; }

        public virtual DbSet<SaveFolder> savefolder { get; set; }

        public virtual DbSet<v_Top> v_top { get; set; }

        public virtual DbSet<v_Tsumi_Page> v_tsumi_page { get; set; }

        public virtual DbSet<LikeData> likedata { get; set; }
        
        public virtual DbSet<v_Letter> v_letter { get; set; }

        public virtual DbSet<v_Friends> v_friends { get;set; }

        public virtual DbSet<v_Return> v_return { get; set; }

        public virtual DbSet<v_Post> v_post { get; set; }

        public virtual DbSet<LoginData> loginData { get; set; }

        public virtual DbSet<Use_Tag> use_tag { get; set; }

        public virtual DbSet<Visionupdate> visionupdate { get; set; }

        public virtual DbSet<FalseChars> falseChars { get; set; }

        public virtual DbSet<Pay_His> pay_his { get; set; }
       
        public virtual DbSet<Pay_please> pay_please { get; set; }

        public virtual DbSet<Coinget> coinget { get; set; }

    }
}
