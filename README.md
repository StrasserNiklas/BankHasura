# BankHasura

## Database
Database was created using Heroku in Harusa, tables where created there (users, transactions, payments) **DISCLAIMER** ONLY this line was added after 17.50

## How to use

Navigate to UI url: https://hasuraui20220328132531.azurewebsites.net/
Go to bank, check F12 is websocket fails (dont know why, happens sometimes with azure)
Login with id (1 for example)
**Disclaimer** For whatever reason, the ids are not loaded with graphql, although with a postman query it works, wasnt able to fix it. It works when you create a payment (to id 2 for example)

If status is not changing from "Pending" to "Done, start the HasuraAPI locally (Azure fun for whatever reason)

## Urls

Websocket connection: ws://hasurawebsocketserver2022032823400.azurewebsites.net/ws
TransactionService: https://hasuraapi20220329010150.azurewebsites.net/ 
404 weil keine Website, Service läuft aber natürlich

UI: https://hasuraui20220328132531.azurewebsites.net/

## GraphQl Endpoint
https://sfr-homework.hasura.app/v1/graphql

Querys can be found inside the projects.
## Why is there an additional websocket API?
The websocket api is used to relay websocket requests between the Blazor UI and Hasura, specifically subscription. For whatever reason, it is not possible to set websocket request headers in browsers (see https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocketoptions.setrequestheader?view=net-6.0 for more information). Unfortunately, this was only clear when almost everything was finished UI wise, so I didn´t want to lose all the work and create another UI. The websocket client/server (lightweight version used in this case) was already implemented by me and a friend at an earlier date, which is why I chose to do it this way.

## Rest Endpoints
### Needed header
**Key:** x-hasura-admin-secret
**Value:** XTWlYYXfTuM7SVx1zzUE1PpnTxnhSRgsTCBe5gFiWPm6gc6wegO6dqh2GwzVgxkU
####  CreatePayment
**POST** https://sfr-homework.hasura.app/api/rest/payment
Body:
```
{
	"sender": 1,
	"recipient": 2,
	"amount": 500,
	"description": "hasuraRest"
}
```

####  UpdatePayment
**PUT** https://sfr-homework.hasura.app/api/rest/payment
Body:
```
{
	"id": 1,
	"status": "Done"
}
```
####  CreateTransaction
**POST** 
https://sfr-homework.hasura.app/api/rest/transaction
Body:
```
{
	"sender": 1,
	"recipient": 2,
	"amount": 500,
	"description": "hasuraRest"
}
```
####  CreateUser
**POST** 
https://sfr-homework.hasura.app/api/rest/user
Body:
```
{
	"name": "Daniel"
}
```




