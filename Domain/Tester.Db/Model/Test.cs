using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using REST.DataCore.Contract.Entity;
using REST.EfCore.Annotation;
using Tester.Db.Model.Client;
using Tester.Db.Model.Statistics;

namespace Tester.Db.Model
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "CA2227")]
    [Table("test", Schema = DbConstant.Scheme.Default)]
    public class Test : ICreatedUtc, IUpdatedUtc, IDeletable
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AuthorId { get; set; }

        [Index]
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Index]
        public DateTime CreatedUtc { get; set; }

        public DateTime UpdatedUtc { get; set; }

        [Index]
        public DateTime? DeletedUtc { get; set; }

        
        [ForeignKey(nameof(AuthorId))]
        public User Author { get; set; }

        public ICollection<TestTopic> TestTopics { get; set; }
        public ICollection<UserTest> UserTests { get; set; }
    }
}