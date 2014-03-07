using System;
using NUnit.Framework;
using Pingring.Utilities;

namespace Pingring.Utilities.ConstructorMapper.Tests
{
    public abstract class BaseTestHarness
    {
        [SetUp]
        protected void Setup()
        {
            Arrange();
            Act();
        }

        [TearDown]
        protected virtual void Cleanup() { }

        protected abstract void Arrange();
        protected abstract void Act();
    }

    public class MappingTests : BaseTestHarness
    {
        private Poco _dbModel;
        private BusinessObject _businessObject;

        protected override void Arrange()
        {
            _dbModel = new Poco
            {
                user_id = 13,
                user_name = "bboettcher",
                comment_count = 42,
                display_name = "Bryan Boettcher",
                registered_date = new DateTime(2014, 03, 07)
            };

            ConstructorMapper.Configure<Poco, BusinessObject>();
        }

        protected override void Act()
        {
            _businessObject = ConstructorMapper.Map<Poco, BusinessObject>(_dbModel);
        }
        
        [Test]
        public void It_will_map_properties()
        {
            Assert.That(_businessObject.UserId, Is.EqualTo(13));
            Assert.That(_businessObject.UserName, Is.EqualTo("bboettcher"));
            Assert.That(_businessObject.CommentCount, Is.EqualTo(42));
            Assert.That(_businessObject.DisplayName, Is.EqualTo("Bryan Boettcher"));
            Assert.That(_businessObject.RegisteredDate, Is.EqualTo(new DateTime(2014, 03, 07)));
        }

        protected override void Cleanup()
        {
            ConstructorMapper.Clear();
        }
    }

    public class Poco
    {
        public int user_id { get; set; }
        public string user_name { get; set; }
        public int comment_count { get; set; }
        public DateTime registered_date { get; set; }
        public string display_name { get; set; }
    }

    public class BusinessObject
    {
        private readonly int _userId;
        private readonly string _userName;
        private readonly int _commentCount;
        private readonly DateTime _registeredDate;
        private readonly string _displayName;

        public BusinessObject(int userId, string userName, int commentCount, DateTime registeredDate, string displayName)
        {
            _userId = userId;
            _userName = userName;
            _commentCount = commentCount;
            _registeredDate = registeredDate;
            _displayName = displayName;
        }

        public int UserId { get { return _userId; } }
        public string UserName { get {  return _userName; } }
        public int CommentCount { get { return _commentCount; } }
        public DateTime RegisteredDate { get { return _registeredDate; } }
        public string DisplayName { get { return _displayName; } }
    }
}
