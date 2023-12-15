using System;

namespace TG_WITH_MAFIA_bot
{
    public interface IRole
    {
        string Name { get; }
        string Description { get; }
        string Fraction { get; }
        int HP { get; }
        int Damage { get; }
        bool isAlive { get; }
        bool GetDamage(int damage);
        bool UseSkill(ref Player target);

        IRole Clone();
    }

    public class RoleBase : IRole
    {
        public virtual string Name { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual string Fraction { get; protected set; }
        public virtual int HP { get; protected set; }
        public virtual int Damage { get; protected set; }
        public virtual bool isAlive { get; protected set; }

        public virtual bool GetDamage(int damage)
        {
            HP -= damage;
            if (HP <= 0) isAlive = false;
            return true;
        }

        public virtual bool UseSkill(ref Player target)
        {
            return true;
        }

        public virtual IRole Clone()
        {
            return new RoleBase
            {
                Name = this.Name,
                Description = this.Description,
                Fraction = this.Fraction,
                HP = this.HP,
                Damage = this.Damage,
                isAlive = this.isAlive
            };
        }
    }

    public class EmptyRole : RoleBase
    {
        public EmptyRole()
        {
            Name = "Пустая роль";
            Description = "Пустая роль. Заглушка. Игрок не должен играть за эту роль";
            Fraction = "Empty";
            HP = 1;
            Damage = 0;
            isAlive = true;
        }

        public override IRole Clone()
        {
            return new EmptyRole
            {
                Name = this.Name,
                Description = this.Description,
                Fraction = this.Fraction,
                HP = this.HP,
                Damage = this.Damage,
                isAlive = this.isAlive
            };
        }
    }

    public class CivilianRole : RoleBase
    {
        public CivilianRole()
        {
            Name = "Мирный житель";
            Description = "Обычный горожанин. Не имеет навыков. Победит, если все убийцы мертвы";
            Fraction = "Civilian";
            HP = 1;
            Damage = 0;
            isAlive = true;
        }

        public override IRole Clone()
        {
            return new CivilianRole
            {
                Name = this.Name,
                Description = this.Description,
                Fraction = this.Fraction,
                HP = this.HP,
                Damage = this.Damage,
                isAlive = this.isAlive
            };
        }
    }

    public class MafiaRole : RoleBase
    {
        public MafiaRole()
        {
            Name = "Мафиози";
            Description = "Мафиозник. Может убить 1 человека за ночь. Победит, если в живых останутся только члены мафиозного семейства";
            Fraction = "Mafia";
            HP = 1;
            Damage = 1;
            isAlive = true;
        }

        public override IRole Clone()
        {
            return new MafiaRole
            {
                Name = this.Name,
                Description = this.Description,
                Fraction = this.Fraction,
                HP = this.HP,
                Damage = this.Damage,
                isAlive = this.isAlive
            };
        }
    }
}
