1. run the web API in visual studio or Rider
2. register a user by executing the following command in the Auth Section:
```bash{
  "id": 0, 
  "username": "Enter your username",
  "password": "Enter your password"
}
```
3. login with the registered user and copy the token
4. Use the token to authorize the user in the swagger UI at the top right corner by typing 
```
bearer <token>
```
5. Now, the user should be able to access and modify user specific data. Errors will be thrown if the user does not have access to specific data.

## Functionalities
- User can register and login
- User can Add/remove cars
- User can List cars and stock levels
- User can update car stock level
- User can search for a car by make and model

## Technologies
- .NET 7
- Entity Framework Core
- SQLite
- Dapper
- BCrypt
- JWT

## Important
- The token in appsettings.json is for testing purposes only. For production it is recommended that I use a more secure method to store the token such as Azure Key Vault.