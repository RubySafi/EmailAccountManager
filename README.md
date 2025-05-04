# EmailAccountManager

**EmailAccountManager** is a simple and practical desktop application for managing which email addresses have been registered on which websites. 
It supports multiple users and enables easy tracking, editing, and backup of account registration data.

## Features

- **Email Registration Tracking**  
  Register websites along with one or more email addresses used for that site.

- **Multi-user Support**  
  Each user has an isolated database, enabling management of separate sets of email accounts (e.g., `userA`, `userB`, etc.).

- **Add / Edit / Delete**  
  You can add new site entries, update existing ones, or remove them.

- **Timestamp Support**  
  Automatically records the date of registration for each entry.

- **Comment and Security Level**  
  Add comments or assign a security level to each site for better context.

- **Backup on Delete**  
  When a user account is deleted, the corresponding database is not erased but moved to a backup folder (`db/backup/`) for safety.

- **Manual Restore**  
  Backed-up user data can be restored by manually moving the corresponding `.db` file back into the `db/` directory.

## Use Case

When changing your primary email address, this tool helps you quickly identify which sites need to be updated. 
This ensures you do not miss critical services during the transition.

Note: Although this application is designed to manage email-based registrations, 
it also supports using phone numbers in place of email addresses.
You can freely enter phone numbers instead of emails if a service uses phone-based login.

## Technologies Used

- WPF (.NET Desktop Application)
- SQLite (via `Microsoft.Data.Sqlite`)
- JSON serialization (via `Newtonsoft.Json`)
- Material Icons from Google

## License

This project is licensed under the **MIT License**.  
Please also refer to the licenses of the following dependencies:

- [Microsoft.Data.Sqlite](https://github.com/dotnet/efcore) - MIT License  
- [Newtonsoft.Json](https://www.newtonsoft.com/json) - MIT License  
- [Material Icons](https://fonts.google.com/icons) - Apache License 2.0

### Material Icons License Notice

Material Icons are licensed under the [Apache License, Version 2.0](http://www.apache.org/licenses/LICENSE-2.0).  
(c) Google LLC. You may obtain a copy of the license at the link above.

## Disclaimer

This software is provided "as is", without warranty of any kind, express or implied.  
The author is not responsible for any damages or data loss resulting from the use of this application.
Use at your own risk.


