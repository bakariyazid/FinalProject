# 📱 MITC QRCODE ATTENDANCE (v2.0)


## 1️⃣ Introduction

* **Project Title:** MITC QRCODE ATTENDANCE
* **Developer Name:** BAKARI YAZID AKANNI
* **Project Idea:** MITC QRCode Attendance is an advanced digital attendance management ecosystem designed to eliminate conventional classroom and workplace tracking inefficiencies. By leveraging a high-security, dynamic QR code verification engine alongside offline synchronization capabilities, the system provides a tamper-proof infrastructure for real-time presence tracking.

---

## 2️⃣ Problem Statement

* **The Problem:** Traditional attendance methods (manual paper sheets, static digital check-ins, or simple proximity cards) are highly vulnerable to fraud, specifically "proxy attendance" where one student signs in on behalf of multiple absent peers. 
* **Who Experiences It:** Educational institutions, administrators, and instructors who lose valuable instructional time managing manual logs and suffer from inaccurate academic tracking data.
* **Why It Matters:** Inaccurate attendance compromises academic integrity, disrupts performance analytics, and dilutes the accountability required in modern professional training frameworks.

## 3️⃣ Project Objectives

The system is designed to achieve the following core benchmarks:
* **Eliminate Proxy Tracking:** Eradicate attendance manipulation through real-time cryptographic verification.
* **Ensure Network Resilience:** Guarantee uninterrupted operation during intermittent server disconnects via offline caching.
* **Streamline Administration:** Automate manual record-keeping into instantly exportable, secure database logs.
* **Optimize User Engagement:** Provide fluid, beautiful, cross-device interfaces that take seconds for instructors and students to operate.

## 4️⃣ System Overview

### How the System Works
1. **Generation:** The instructor initiates an active tracking session from their portal, signaling the server to begin generating token cycles.
2. **Scanning:** The dynamic token is translated into a QR code on screen. Students scan the code using their mobile interfaces, securely capturing token metadata and device tracking data.
3. **Verification:** The backend validates structural integrity and timestamps before officially storing the authorized attendance record.

### Main Features
* **Time-Windowed Rotation Series:** The core anti-fraud engine. Generated QR codes rely on a strict time-windowed rotation series that cycles tokens continuously. Shared screenshots or late capture attempts are rendered instantly invalid.
* **Offline Data Syncing:** Built to withstand network drops. If local connection fails, the application switches to an offline queue state, tracking status records locally and performing a silent, robust synchronization once the network stabilizes.
* **Automated Communication:** Instant triggers for administrative notifications and system verification alerts.

### Target Users
* **System Administrators:** For system configuration, auditing, and role-based privilege management.
* **Instructors / Lecturers:** To host sessions, monitor real-time check-ins, and extract analytical reports.
* **Students:** For fast, cross-device mobile verification checks.

## 5️⃣ Technology Used

| Layer | Tools & Frameworks | Purpose |
| :--- | :--- | :--- |
| **Frontend UI** | HTML5, CSS3, Razor, JavaScript | Modern UI built on Glassmorphism design trends with responsive animations. |
| **Backend Core** | C# / ASP.NET Core MVC | Enterprise architecture handling routing, secure controllers, and business logic. |
| **Data Access** | Entity Framework Core | Repository-pattern abstraction for safe, optimized database interaction. |
| **Database Engine** | MySQL | Relational data engine managing users, transactional tokens, and sync logs. |
| **External APIs** | **Brevo API** | Powering reliable, automated system emails and verification notifications. |
| **Dev Environment** | Ngrok | Local tunneling used to securely test cross-device mobile-to-laptop integration. |

---

## 8️⃣ Conclusion

### What I Learned
Through the lifecycle of developing this project, I significantly advanced my full-stack engineering capabilities under the National Skills Qualification (NSQ) units. I mastered structured backend development utilizing **ASP.NET Core MVC** and **Entity Framework Core**, designed secure relational schemas in **MySQL**, implemented strict defensive security token barriers, and navigated external network integration structures like the **Brevo API** and **Ngrok** tunneling pipelines.

### Possible Improvements
* **Biometric Redundancy:** Integrating secondary facial verification or fingerprint biometric safeguards layered over the dynamic QR code scanner.
* **Advanced Analytics Dashboards:** Building AI-driven predictive modeling charts on the instructor dashboard to automatically flag at-risk attendance patterns early in the semester.
