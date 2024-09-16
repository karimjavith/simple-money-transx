# Moneybox Money Withdrawal

The solution contains a .NET core library (Moneybox.App) which is structured into the following 3 folders:

* Domain - this contains the domain models for a user and an account, and a notification service.
* Features - this contains two operations, one which is implemented (transfer money) and another which isn't (withdraw money)
* DataAccess - this contains a repository for retrieving and saving an account (and the nested user it belongs to)

## The task

The task is to implement a money withdrawal in the WithdrawMoney.Execute(...) method in the features folder. For consistency, the logic should be the same as the TransferMoney.Execute(...) method i.e. notifications for low funds and exceptions where the operation is not possible. 

As part of this process however, you should look to refactor some of the code in the TransferMoney.Execute(...) method into the domain models, and make these models less susceptible to misuse. We're looking to make our domain models rich in behaviour and much more than just plain old objects, however we don't want any data persistance operations (i.e. data access repositories) to bleed into our domain. This should simplify the task of implementing WithdrawMoney.Execute(...).

## Guidelines

* The test should take about an hour to complete, although there is no strict time limit
* You should fork or copy this repository into your own public repository (Github, BitBucket etc.) before you do your work
* Your solution must build and any tests must pass
* You should not alter the notification service or the the account repository interfaces
* You may add unit/integration tests using a test framework (and/or mocking framework) of your choice
* You may edit this README.md if you want to give more details around your work (e.g. why you have done something a particular way, or anything else you would look to do but didn't have time)

Once you have completed test, zip up your solution, excluding any build artifacts to reduce the size, and email it back to our recruitment team.

Good luck!

## **Karim Notes**

* Approached the problem with TDD setup.
* Lost sometime during the kick off as there were some conflict with my vscode and dotnet sdk setup :disappointed:
* Have used xunit and moq for testing purposes.
* The failing tests highlighted that there is a bug in transfer service withdrawn money calculation.
* After fixing the tests (for both existing and new features), I realised that there are some repetitions with the balance/withdrawn calculation in the methods. So, updated the Account entity with some calculation methods for debit and credit which should hopefully make the method bit readable.
* I find the hard code values such as limits for pay-in and minimum balance should be in configs rather than in-line.
* I am happy with the existing solution given the time spent. However, I am keen to abstract away the validation to a separate class if it is intended to break the rule of 2.
* Thanks for taking your time to review this work. Please let me know your comments and feedback.
