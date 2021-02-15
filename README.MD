# AutoAbsenSKI ![.NET](https://github.com/fahminlb33/AutoAbsenSKI/workflows/.NET/badge.svg)

This app is an automation tool to send my work progress from JIRA to email.
While sending email every month is not much of a hassle, but if it can be
automated it will help ease another work for me.

## Syntax

```
AutoAbsenSKI:
  Automatically create JIRA report for SKI administration and optionally
  create a scheduled task to automatically send report every 15th of each
  month.

Usage:
  AutoAbsenSKI [options]

Options:
Options:
  -s, --settings    Initialize/open settings
  -i, --install     Install Task Scheduler to send report automatically
  -d, --dry-run     Generate report without sending it
  --version         Show version information
  -?, -h, --help    Show help and usage information
```

Note: Scheduler only works in Windows.

## Compatibility

This project is made using .NET 5, so you'll need .NET 5 Runtime or SDK to be
able to run this software. This app is compatible with Windows, OSX, and Linux.

## How To

1. Download the binaries from Release page or clone this repo.
2. Run `autoabsenski --settings` to open settings file.
3. Fill up all the necessary fields (example below).
4. Save the settings file, then run `autoabsenski` without any arguments.
5. The software will launch a Chromium browser using Pupeteer in the background
   and send the report as email.
6. If you want to add Task Scheduler, execute `autoabsenski --install-scheduler`.

### Example Configuration

```
{
  "ChromiumPath": "",
  "Viewport": {
    "Width": 1920,
    "Height": 1080
  },
  "AtlassianAccount": {
    "Email": "EMAIL-HERE",
    "Password": "PASSWORD-HERE"
  },
  "EmailAccount": {
    "Email": "GMAIL-HERE",
    "Password": "APP-PASSWORD-HERE",
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Ssl": true
  },
  "EmployeeName": "Fahmi Noor Fiqri",
  "Recipients": [ "meow@domain.com", "bark@domain.com" ]
}
```

Note:

- If `ChromiumPath` is empty, the software will download it automatically.
- If `Viewport` is empty, the software will use 1920x1080 size.
- If you're using Gmail, use [App Password](https://myaccount.google.com/apppasswords)
  and not your current password.
