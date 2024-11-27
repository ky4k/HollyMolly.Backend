# HollyMolly.Backend
### Table of Contents
* [Introduction](#introduction)
* [Links](#links)
* [Launch](#launch)
* [Technology](#technology)
* [External Services](#external-services)
* [Features](#features)

### Introduction
This is the backend part of the Holly Molly Online Store, developed as a [Team Challenge](https://teamchallenge.io/) project.
### Links
- [Website (frontend)](https://holly-molly.vercel.app)
- [Website (backend)](https://holly-molly-back.tyavd.net)
- [Source code (frontend)](https://github.com/Nikita80230/HollyMolly)
- [Source code (backend)](https://github.com/ky4k/HollyMolly.Backend)

### Launch
- Clone the repository `git clone https://github.com/ky4k/HollyMolly.Backend`
- Open **WebAPI/appsettings.json** file and fill in the configuration details (a connection string to a MS SQL Server, credentials to external services, defaults etc.).
- Navigate to the WebAPI directory and run the application using: `dotnet run`.

### Technology
- ASP.NET 8
- MS SQL Server
- Entity Framework Core
- ClosedXML
- FluentValidation
- Serilog
- xUnit
- NSubstitute

### External Services
- [Google OAuth](https://developers.google.com/identity/protocols/oauth2) - Used for OAuth 2.0 / OIDC authentication.
- [Stripe](https://docs.stripe.com/) - Handles payment processing.
- [Brevo](https://developers.brevo.com/) - Sends email notifications.
- [Nova Poshta](https://developers.novaposhta.ua/documentation) - Provides delivery tracking and logistics information for shipments.

### Features
For full API documentation, see: [Holly Molly API Documentation](https://holly-molly-back.tyavd.net/swagger/index.html)
* User registration and authentication with email and password or via Google OAuth.
* Manage products and categories.
* Upload and manage product images.
* Leaving feedback for products.
* Wishlists.
* Creating orders.
* Orders' payments processing.
* Tracking orders.
* Email notifications for various events.
* Comprehensive statistics with export to the Excel function.
