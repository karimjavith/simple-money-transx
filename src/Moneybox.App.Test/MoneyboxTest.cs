using Moq;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;

namespace Moneybox.App.Test
{
    public class MoneyboxTest
    {
        private Guid accountIdFrom;
        private Guid accountIdTo;

        private User john;
        private User peter;

        private Account fromAccount;
        private Account toAccount;

        private Mock<IAccountRepository> mockAccountRepository;
        private Mock<INotificationService> mockNotificationService;

        public MoneyboxTest()
        {
            // Init: Mock accountRepository and mock notificationService
            mockAccountRepository = new Mock<IAccountRepository>();
            mockNotificationService = new Mock<INotificationService>();
        }

        // Test setup for accounts and users entities
        private void SetupBasicAccounts(decimal fromAccountBalance, decimal toAccountPaidIn = 0m, decimal toAccountBalance = 200m)
        {
            john = new User
            {
                Name = "John Doe",
                Email = "john.doe@gmail.com",
                Id = Guid.NewGuid()
            };

            peter = new User
            {
                Name = "Peter Fiddle",
                Email = "peter.fiddle@gmail.com",
                Id = Guid.NewGuid()
            };

            accountIdFrom = Guid.NewGuid();
            accountIdTo = Guid.NewGuid();

            fromAccount = new Account
            {
                Id = accountIdFrom,
                Balance = fromAccountBalance,
                User = john
            };

            toAccount = new Account
            {
                Id = accountIdTo,
                Balance = toAccountBalance,
                PaidIn = toAccountPaidIn,
                User = peter
            };

            // Setup mock behavior
            mockAccountRepository.Setup(x => x.GetAccountById(accountIdFrom)).Returns(fromAccount);
            mockAccountRepository.Setup(x => x.GetAccountById(accountIdTo)).Returns(toAccount);
        }

        [Fact]
        public void ShouldThrowException_WhenInsufficientFunds_ForTransfer()
        {
            // Arrange
            SetupBasicAccounts(50m); // John has only 50 in his account
            var transferMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                transferMoney.Execute(accountIdFrom, accountIdTo, 100m)); // Trying to transfer 100, which should fail

            // Assert
            Assert.Equal("Insufficient funds to make transfer", exception.Message);
        }

        [Fact]
        public void ShouldNotify_WhenFundsLowAfterTransfer()
        {
            // Arrange
            SetupBasicAccounts(100m); // John has 100 in his account
            var transferMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);

            // Act
            transferMoney.Execute(accountIdFrom, accountIdTo, 99m); // Transferring 99 leaves only 1, which is "low funds"

            // Assert: John should receive a low funds notification
            mockNotificationService.Verify(x => x.NotifyFundsLow(john.Email), Times.Once);
            mockAccountRepository.Verify(x => x.Update(It.IsAny<Account>()), Times.Exactly(2));
        }

        [Fact]
        public void ShouldThrowException_WhenPayInLimitExceeded()
        {
            // Arrange: Peter is close to the pay-in limit (3800 out of 4000)
            SetupBasicAccounts(1000m, 3000m);
            var transferMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                transferMoney.Execute(accountIdFrom, accountIdTo, 1001m)); // Transferring 1001 exceeds the 4000 limit

            // Assert
            Assert.Equal("Account pay in limit reached", exception.Message);
        }

        [Fact]
        public void ShouldNotify_WhenApproachingPayInLimit()
        {
            // Arrange: Peter has paid in 3500 (close to the 4000 limit)
            SetupBasicAccounts(1000m, 3500m);
            var transferMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);

            // Act
            transferMoney.Execute(accountIdFrom, accountIdTo, 400m); // Transfer brings Peter's total paid-in to 3900, approaching the limit

            // Assert: Peter should receive a notification about approaching the limit
            mockNotificationService.Verify(x => x.NotifyApproachingPayInLimit(peter.Email), Times.Once);
            mockAccountRepository.Verify(x => x.Update(It.IsAny<Account>()), Times.Exactly(2)); // Both accounts should be updated
        }

        [Fact]
        public void ShouldUpdateBalancesCorrectly_AfterTransfer()
        {
            // Arrange: Set up accounts with initial balances
            SetupBasicAccounts(100m, toAccountBalance: 200m);
            var transferMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);

            // Act: Transfer 50 from John's account to Peter's account
            transferMoney.Execute(accountIdFrom, accountIdTo, 50m);

            // Assert: Check balances and transaction details
            Assert.Equal(50m, fromAccount.Balance); // John's balance should decrease
            Assert.Equal(250m, toAccount.Balance); // Peter's balance should increase
            Assert.Equal(50m, fromAccount.Withdrawn); // John's withdrawn should increase
            Assert.Equal(50m, toAccount.PaidIn); // Peter's paid-in should increase
        }


        [Fact]
        public void ShouldThrowException_WhenInsufficientFunds_ForWithdraw()
        {
            // Arrange 
            SetupBasicAccounts(50m); // John has only 50 in his account
            var withdrawMoney = new WithdrawMoney(mockAccountRepository.Object, mockNotificationService.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                withdrawMoney.Execute(accountIdFrom, 100m)); // Trying to transfer 100, which should fail

            // Assert
            Assert.Equal("Insufficient funds to withdraw from", exception.Message);
        }

        [Fact]
        public void ShouldUpdateBalancesCorrectly_AfterWithDraw()
        {
            // Arrange
            SetupBasicAccounts(100m);
            var withdrawMoney = new WithdrawMoney(mockAccountRepository.Object, mockNotificationService.Object);

            // Act: WithDraw 50 from John's account
            withdrawMoney.Execute(accountIdFrom, 50m);

            // Assert: Check balances and transaction details
            Assert.Equal(50m, fromAccount.Balance); // John's balance should decrease
            Assert.Equal(50m, fromAccount.Withdrawn); // John's withdrawn should increase
        }
    }
}
