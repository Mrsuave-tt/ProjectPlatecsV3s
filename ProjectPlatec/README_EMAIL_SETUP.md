# Email Configuration Guide

## Setting Up Email for Student Credentials

The application sends student login credentials via email. Follow these steps to configure email:

### For Gmail Users:

1. **Enable 2-Factor Authentication** on your Gmail account
   - Go to Google Account Settings → Security
   - Enable 2-Step Verification
    
2. **Generate an App Password**
   - Go to Google Account → Security → 2-Step Verification
   - Scroll down to "App passwords"
   - Select "Mail" and "Other (Custom name)"
   - Enter "ProjectPlatec" as the name
   - Copy the generated 16-character password

3. **Update appsettings.json**
   ```json  w
   "EmailSettings": {
     "SmtpServer": "smtp.gmail.com",
     "SmtpPort": 587,
     "SenderEmail": "your-email@gmail.com",
     "SenderName": "ProjectPlatec", 
     "SenderPassword": "your-16-character-app-password",
     "EnableSsl": true
   },
   "AppSettings": {w
     "WebsiteUrl": "https://localhost:7223"
   }
   ```

### For Other Email Providers:

#### Outlook/Hotmail:
```json
"EmailSettings": {
  "SmtpServer": "smtp-mail.outlook.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@outlook.com",
  "SenderName": "ProjectPlatec",
  "SenderPassword": "your-password",
  "EnableSsl": true
}
```

#### Yahoo:
```json
"EmailSettings": {
  "SmtpServer": "smtp.mail.yahoo.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@yahoo.com",
  "SenderName": "ProjectPlatec",
  "SenderPassword": "your-app-password",
  "EnableSsl": true
}
```

### Important Notes:

- **Never use your regular Gmail password** - Always use an App Password for Gmail
- Make sure the `WebsiteUrl` in `AppSettings` matches your actual website URL
- For production, use environment variables or Azure Key Vault for sensitive credentials
- Test the email configuration by creating a test student account

### Troubleshooting:

1. **"Authentication failed" error:**
   - For Gmail: Make sure you're using an App Password, not your regular password
   - Check that 2-Factor Authentication is enabled

2. **"Connection timeout" error:**
   - Check your firewall settings
   - Verify SMTP server and port are correct
   - Try using port 465 with SSL instead of 587 with TLS

3. **"Email could not be sent" error:**
   - Check the application logs for detailed error messages
   - Verify all email settings in appsettings.json are correct
   - Test your email credentials with an email client first

