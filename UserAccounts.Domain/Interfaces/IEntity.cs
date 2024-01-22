namespace UserAccounts.Domain.Interfaces
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public interface IEntity
    {
        [Key]
        public int Id { get; set; }
    }
}
