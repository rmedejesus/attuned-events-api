# Event Ticketing RESTful API Service

**Introduction**

This is a backend API for an event ticketing developed with C#.\
The programming language and tools used for developing this application are:\
- C# 8.0 with ASP.NET Core Web API Framework
- Microsoft Visual Studio 2022
- MongoDB Compass (for database storage and GUI)
- Docker Desktop 4.27.2 (for building images and containerizing the API and database services)
- Swagger/Postman (for testing the API endpoints)

The application and code structure is built within an MVC (Model-View-Controller) pattern. The application was also built within a docker-compose yaml file to easily build and run the API service.

**Building and Running the Application**

**1. Via Docker**
- from the root folder, open the CMD or Command Prompt 
- run the command line `docker compose up --build -d` to build the docker images and containers for the API and database services.
- after it has successfully built the services, run `docker ps` to check the running services and their currently assigned ports. To check the current running port for the API, the image name is `attunedeventsapi` and the working port assignment is on the `8080/tcp`
- there are two ways to test the API endpoints, the first one is done by curling to `http://localhost:<port>/swagger/index.html` and test the several endpoints from the UI given by the swagger documentation. The second one is to use the endpoint URLs via Postman.

**2. Via Visual Studio 2022 IDE**
- open the project solution in the Visual Studio IDE
- I have currently set the build configuration to Docker Compose so we could debug and run the application from the IDE. Once the project solution loads, we could just click the `Docker Compose` button and the IDE will start compiling the application and build the Docker images and containers.
- Once the application is running, it will automatically open the swagger URL for the application and from there on, we could proceed on using the API endpoints via swagger or Postman.

If you have the MongoDB Compass GUI, you can check for the created database `attunedDB` together with the created collections for `events` and `reservations`. The `events` collection already has the data for all the available events. We can connect to the database server by connecting to `mongodb://host.docker.internal:27017/` after the Docker container has successfully built and ran.

**Testing the Application**

The API endpoints have the following structure:
- `GET http://localhost:<port>/events` this is the endpoint for getting the list of events and their currently available tickets.
- `POST http://localhost:<port>/events/{eventId}/reservations` this is the endpoint for creating a new ticket reservation for a specific event.
- `PATCH http://localhost:5068/events/{eventId}/reservations/{reservationId}` this is the endpoint for updating your current ticket reservation for a specific event.
- `DELETE http://localhost:5068/events/{eventId}/reservations/{reservationId}` this is the endpoint for deleting a reservation for a specific event.

To be able to get the corresponding `{eventId}`, we could check the `eventId` property from the JSON response body from the `GET` request, or by going to the MongoDB Compass GUI and connect to `mongodb://host.docker.internal:27017/` to check the data. The `{reservationId}` is being return on the JSON response body after we have successfully created, updated, or deleted a reservation entity. Alternatively, we could also check the reservation data through the MongoDB Compass GUI.

**Other Notes**

I have applied constraints and error messages to be able to handle the API results effectively:
- When creating a ticket reservation, the input cannot exceed the amount of tickets available. If the user enters a greater amount than the currently available, then it will throw a `400 Bad Request` together with an error message to helpfully inform the user what has been wrong. The same error goes for when the user inputs a character other than an integer value.
-  When a user updates a current reservation, the same constraints from the above also apply here. Also, when a user updates the amount of tickets, it will automatically reflect and update the total number of available tickets from the specific event. For example, if the initial reservation amount was 10, and the total available tickets are 100, then upon creating the reservation, the response body would show the new available tickets, which should be 90. Now, when the user updates the amount of 10 reservations to 15, then the response body after updating will now show 85. And if the user updates the amount again from 15 to 10, then it will update the total available tickets again from 85 to 90.
- When a user deletes a specific reservation, the amount will be returned to the total available tickets for the specific event.