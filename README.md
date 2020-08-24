# goodfood-Dotnet-core-3.1-MSSQL-without-EF-and-Dapper

To create DotnetCore 3.1 project with MSSQL without the EF and Dapper.
Project is all about to create and manage the Recipe.

Database have five tables;-
1. User 
2. Dish Category
3. Dish it self
4. Ingredients
5. Recipe

Every User have there own Dish Category, Dish, Ingredients and Recipes.
But user can look other users recipes as well.

## Note: 
You must have / install dotnet core 3.1 and MSSQL Server 18 on your machine

## How to Run 
1. Clone this Repository and Extract it to a Folder.
2. Create database name 'GoodFoodDB', using Microsoft SQL Server Management studio, or any other way
3. Change the Connection Strings for in the API/appsettings.json
4. Run the following command on Powershell in Main directory (Not API)
- dotnet restore
5. Run the command in API/
- dotnet run or dotnet watch run

## Questions? Bugs? Suggestions for Improvement?
[Get in touch with me](rajpalbains82@gmail.com)
