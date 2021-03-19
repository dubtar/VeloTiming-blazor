using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VeloTiming.Server.Data;
using VeloTiming.Server.Hubs;
using VeloTiming.Server.Logic;
using VeloTiming.Server.Services;
using Xunit;

namespace VeloTiming.Server.Tests
{
    public class MainServiceTests
    {
        private RaceLogic sut;
        private DateTime fakeNow = DateTime.UtcNow;
        private readonly Mock<IServiceProvider> serviceProvider;

        public MainServiceTests()
        {
            // Mock TaskQueue to run synchronously
            var taskQueueMock = new Mock<IBackgroundTaskQueue>();
            taskQueueMock.Setup(t => t.QueueBackgroundWorkItem(It.IsAny<Func<CancellationToken, Task>>()))
                .Callback<Func<CancellationToken, Task>>(action =>
                {
                    action(new CancellationToken()).Wait();
                });

            // Mock IHub
            var iHubContextMock = new Mock<IHubContext<ResultHub, IResultHub>>();
            var mockClients = new Mock<IHubClients<IResultHub>>();
            mockClients.Setup(c => c.All).Returns(Mock.Of<IResultHub>());
            iHubContextMock.Setup(h => h.Clients).Returns(() => mockClients.Object);

            // mock IServiceProvider
            serviceProvider = new Mock<IServiceProvider>();

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IHubContext<ResultHub, IResultHub>)))
                .Returns(iHubContextMock.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IBackgroundTaskQueue)))
                .Returns(taskQueueMock.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(IResultLogic)))
                .Returns(new Mock<IResultLogic>().Object);

            // mock timeService
            var timeServiceMock = new Mock<ITimeService>();
            timeServiceMock.Setup(t => t.Now).Returns(() => fakeNow);
            serviceProvider.Setup(x => x.GetService(typeof(ITimeService))).Returns(timeServiceMock.Object);
        }

        private void CreateSut()
        {
            // create System under test
            sut = new RaceLogic(serviceProvider.Object);

            sut.SetActiveStart(new Start
            {
                Id = 12,
                Name = "Test start",
                Race = new Race("Test race", fakeNow),
                IsActive = true,
                RealStart = new DateTime(2019, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Type = StartType.Laps
            }, new Dictionary<string, string> {
                { "1", "Пупкин Вася"},
                { "2", "Иванов Петя"},
                { "3", "Сюткин Павел"},
                { "15", "Зюзин Архип"}
            }).Wait();
        }

        [Fact]
        public void AddNumberAndTime_CalculatePlaceAndLaps_Pass()
        {
            CreateSut();
            DateTime start = new DateTime(2019, 1, 1, 1, 1, 1, DateTimeKind.Utc);

            sut.AddNumberAndTime("1", start.AddSeconds(100), "rfid");

            Assert.Single(sut.GetResults());
            AssertResult("1", 1, 1);

            sut.AddNumberAndTime("2", start.AddSeconds(110), "rfid");
            AssertResult("2", 2, 1);
            sut.AddNumberAndTime("1", start.AddSeconds(220), "rfid");
            AssertResult("1", 1, 2);
            sut.AddNumberAndTime("2", start.AddSeconds(230), "rfid");
            AssertResult("2", 2, 2);
            sut.AddNumberAndTime("2", start.AddSeconds(330), "rfid");
            AssertResult("2", 1, 3);
            sut.AddNumberAndTime("1", start.AddSeconds(340), "rfid");
            AssertResult("1", 2, 3);
            sut.AddNumberAndTime("2", start.AddSeconds(430), "rfid");
            AssertResult("2", 1, 4);
            sut.AddNumberAndTime("2", start.AddSeconds(530), "rfid");
            AssertResult("2", 1, 5);
            sut.AddNumberAndTime("1", start.AddSeconds(540), "rfid");
            AssertResult("1", 2, 4);
        }

        [Fact]
        public void AddNumberAddTime_Pass()
        {
            CreateSut();
            const string s = "source";
            DateTime start = DateTime.UtcNow.AddSeconds(-1000);
            fakeNow = start.AddSeconds(99);
            sut.AddNumber("1", s);
            AssertResult("1", 1, 1);
            sut.AddNumber("2", s);
            AssertResult("2", 2, 1);
            sut.AddNumber("3", s);
            AssertResult("3", 3, 1);
            CheckPlaces();

            sut.AddTime(start.AddSeconds(100), s);
            CheckPlaces();
            sut.AddTime(start.AddSeconds(110), s);
            CheckPlaces();
            sut.AddTime(start.AddSeconds(113), s);
            AssertResult("3", 3, 1);
            CheckPlaces();
            sut.AddTime(start.AddSeconds(203), s);
            AssertResult(null, -1, -1);
            fakeNow = start.AddSeconds(205);
            sut.AddNumber("3", s);
            AssertResult("3", 1, 2);
            fakeNow = start.AddSeconds(206);
            sut.AddNumber("2", s);
            sut.AddTime(start.AddSeconds(206), s);
            AssertResult("2", 2, 2);
        }
        private void CheckPlaces()
        {

            int index = 1;
            foreach (var mark in sut.GetResults())
            {
                Assert.Equal(index, mark.Place);
                index++;
            }
        }

        [Fact]
        public void AddNumbersAndTimes2_Pass()
        {
            CreateSut();
            const string s = "source";
            DateTime start = DateTime.UtcNow.AddSeconds(-1000);
            fakeNow = start.AddSeconds(3);
            sut.AddTime(fakeNow.AddSeconds(-1), s);
            fakeNow = fakeNow.AddSeconds(1);
            sut.AddTime(fakeNow.AddMilliseconds(-500), s);
            fakeNow = fakeNow.AddSeconds(1);
            sut.AddNumber("2", s);
            fakeNow = fakeNow.AddSeconds(1);
            sut.AddNumber("3", s);
            AssertResult("3", 2, 1);

            fakeNow = fakeNow.AddSeconds(5);
            sut.AddNumber("15", s);
            fakeNow = fakeNow.AddSeconds(1);
            sut.AddTime(fakeNow.AddSeconds(-2), s);
            AssertResult("15", 3, 1);


            fakeNow = fakeNow.AddSeconds(5);
            sut.AddNumber("1", s);
            fakeNow = fakeNow.AddSeconds(1);
            sut.AddTime(fakeNow.AddSeconds(-2), s);
            AssertResult("1", 4, 1);


            // Lap 2
            fakeNow = fakeNow.AddSeconds(65);
            sut.AddNumber("3", s);
            fakeNow = fakeNow.AddSeconds(1);
            sut.AddTime(fakeNow.AddSeconds(-2), s);
            AssertResult("3", 1, 2);

            fakeNow = fakeNow.AddSeconds(3);
            sut.AddNumber("2", s);
            fakeNow = fakeNow.AddSeconds(1);
            sut.AddTime(fakeNow.AddSeconds(-2), s);
            AssertResult("2", 2, 2);


            fakeNow = fakeNow.AddSeconds(30);
            sut.AddNumber("1", s);
            fakeNow = fakeNow.AddSeconds(1);
            sut.AddTime(fakeNow.AddSeconds(-2), s);
            AssertResult("1", 3, 2);

            // Lap 3
            fakeNow = fakeNow.AddSeconds(65);
            sut.AddNumber("3", s);
            fakeNow = fakeNow.AddSeconds(1);
            sut.AddTime(fakeNow.AddSeconds(-2), s);
            AssertResult("3", 1, 3);
        }

        [Fact]
        public void AddTimeAddNumber_Pass()
        {
            CreateSut();
            const string s = "source";
            DateTime start = DateTime.UtcNow.AddSeconds(-1000);
            fakeNow = start.AddSeconds(99);
            sut.AddTime(start.AddSeconds(98), s);
            AssertResult(null, -1, -1);
            sut.AddTime(start.AddSeconds(99), s);
            AssertResult(null, -1, -1);
            sut.AddTime(start.AddSeconds(100), s);
            AssertResult(null, -1, -1);
            var results = sut.GetResults().ToArray();
            Assert.Equal(3, results.Length);

            fakeNow = start.AddSeconds(103);
            sut.AddNumber("1", s);
            AssertResult(null, -1, -1);
            AssertResult(sut.GetResults().First(), "1", 1, 1);

            fakeNow = start.AddSeconds(105);
            sut.AddNumber("2", s);
            AssertResult(null, -1, -1);
            AssertResult(sut.GetResults().Skip(1).First(), "2", 2, 1);
        }

        private void AssertResult(Result result, string number, int place, int lap)
        {
            Assert.Equal(number, result.Number);
            Assert.Equal(place, result.Place);
            Assert.Equal(lap, result.Lap);
        }
        private void AssertResult(string number, int place, int lap)
        {
            var result = sut.GetResults().Last();
            AssertResult(result, number, place, lap);
        }
    }
}
