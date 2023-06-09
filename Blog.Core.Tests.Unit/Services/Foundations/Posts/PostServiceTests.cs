﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Blog.Core.Brokers.DateTimes;
using Blog.Core.Brokers.Loggings;
using Blog.Core.Brokers.Storages;
using Blog.Core.Models.Posts;
using Blog.Core.Services.Foundations.Posts;
using Microsoft.Data.SqlClient;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;
using Xunit;

namespace Blog.Core.Tests.Unit.Services.Foundations.Posts
{
    public partial class PostServiceTests
    {
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly IPostService postService;

        public PostServiceTests()
        {
            this.storageBrokerMock = new Mock<IStorageBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();

            this.postService = new PostService(storageBroker: this.storageBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object);
        }

        private static string GetRandomMessage() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static SqlException GetSqlException() =>
            (SqlException)FormatterServices.GetUninitializedObject(typeof(SqlException));
        public static TheoryData MinutesBeforeOrAfter()
        {
            int randomNumber = GetRandomNumber();
            int randomNegativeNumber = GetRandomNegativeNumber();

            return new TheoryData<int> {
                randomNumber,
                randomNegativeNumber
            };
        }

        public static TheoryData InvalidMinuteCases()
        {
            int randomMoreThanMinuteFromNow = GetRandomNumber();
            int randomMoreThanMinuteBeforeNow = GetRandomNegativeNumber();

            return new TheoryData<int>
            {
                randomMoreThanMinuteFromNow,
                randomMoreThanMinuteBeforeNow
            };
        }

        //public static IEnumerable<object[]> InvalidMinuteCases()
        //{
        //    int randomMoreThanMinuteFromNow = GetRandomNumber();
        //    int randomMoreThanMinuteBeforeNow = GetRandomNegativeNumber();

        //    return new List<Object[]>
        //    {
        //        new object[] { randomMoreThanMinuteFromNow },
        //        new object[] { randomMoreThanMinuteBeforeNow }
        //    };
        //}

        private static int GetRandomNegativeNumber() =>
            -1 * new IntRange(min: 2, max: 10).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static Post CreateRandomPost() =>
            CreatePostFiller(dates: GetRandomDateTimeOffset()).Create();

        private static IQueryable<Post> CreateRandomPosts() =>
            CreatePostFiller(dates: GetRandomDateTimeOffset())
                .Create(GetRandomNumber())
                    .AsQueryable();

        private static Post CreateRandomPost(DateTimeOffset dates) =>
            CreatePostFiller(dates: dates).Create();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static Post CreateRandomModifyPost(DateTimeOffset dates)
        {
            int randomDaysInPast = GetRandomNegativeNumber();
            Post randomPost = CreateRandomPost(dates);

            randomPost.CreatedDate = randomPost.CreatedDate.AddDays(randomDaysInPast);

            return randomPost;
        }

        private static Filler<Post> CreatePostFiller(DateTimeOffset dates)
        {
            var filler = new Filler<Post>();
            filler.Setup()
                   .OnType<DateTimeOffset>().Use(dates);

            return filler;
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);
    }
}