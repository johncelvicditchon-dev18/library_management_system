-- ============================================================
--  TCGC Library Management System - account tables
--
--  NORMALISED ACCOUNT DESIGN
--    account_table       (account_id, name, username, password)
--    account_role_table  (role_id, account_id, role, status, date_created)
--    institute_table     (institute_id, institute_code, institute_name)
--    program_table       (program_id, program_name, institute_id)
--    student_info_table  (student_info_id, account_id, school_id,
--                         last_name, first_name, middle_initial, program_id,
--                         year_level, institute_id, contact_number, address)
--    faculty_info_table  (faculty_info_id, account_id, institute_id,
--                         contact_number)
--    staff_info_table    (staff_info_id, account_id, role_id, office, contact_number)
--    librarian_info_table (librarian_info_id, account_id, role_id, contact_number)
--
--  Every table has its own surrogate primary key, and every link between
--  the tables is a foreign key (no institute name or program name is
--  copied into the profile rows).
--  New accounts start as PENDING.
--  date_created is TEXT, stored like: OCTOBER 24, 2026
--  Roles offered by signup: LIBRARIAN, FACULTY, STAFF, STUDENT
-- ============================================================

CREATE DATABASE IF NOT EXISTS `libsys_database`
    DEFAULT CHARACTER SET utf8mb4;

USE `libsys_database`;

CREATE TABLE IF NOT EXISTS `account_table` (
    `account_id` INT AUTO_INCREMENT PRIMARY KEY,
    `name`       VARCHAR(250) NOT NULL,
    `username`   VARCHAR(250) NOT NULL,
    `password`   VARCHAR(255) NOT NULL,
    UNIQUE KEY `ux_account_username` (`username`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

CREATE TABLE IF NOT EXISTS `account_role_table` (
    `role_id`      INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`   INT         NOT NULL,
    `role`         VARCHAR(20) NOT NULL,
    `status`       VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    `date_created` VARCHAR(30) NOT NULL,
    CONSTRAINT `fk_account_role`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- Lookup table: the signup dropdown is filled from here.
-- INSERT IGNORE keeps any name the school renames later.
CREATE TABLE IF NOT EXISTS `institute_table` (
    `institute_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `institute_code` VARCHAR(10)  NOT NULL,
    `institute_name` VARCHAR(120) NOT NULL,
    UNIQUE KEY `ux_institute_code` (`institute_code`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

INSERT IGNORE INTO `institute_table` (`institute_code`, `institute_name`) VALUES
    ('ICS',  'INSTITUTE OF COMPUTER STUDIES'),
    ('IAS',  'INSTITUTE OF ARTS AND SCIENCES'),
    ('ICJE', 'INSTITUTE OF CRIMINAL JUSTICE EDUCATION'),
    ('IBFS', 'INSTITUTE OF BUSINESS AND FINANCE STUDIES'),
    ('IHS',  'INSTITUTE OF HEALTH SCIENCES'),
    ('ITE',  'INSTITUTE OF TEACHER EDUCATION');

-- PROGRAM lookup: a program belongs to one institute, so the same program
-- name is stored once per institute. signup.cs registers a program the
-- first time somebody types it, then reuses program_id from then on.
CREATE TABLE IF NOT EXISTS `program_table` (
    `program_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `program_name` VARCHAR(120) NOT NULL,
    `institute_id` INT          NOT NULL,
    UNIQUE KEY `ux_program_name_institute` (`program_name`, `institute_id`),
    CONSTRAINT `fk_program_institute`
        FOREIGN KEY (`institute_id`) REFERENCES `institute_table` (`institute_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- ------------------------------------------------------------
-- ROLE PROFILE TABLES
--   student_info_table, faculty_info_table, staff_info_table,
--   librarian_info_table
-- Each profile row belongs to one account (account_id). The two
-- library roles also point at their exact role row (role_id), so
-- the role name, status and date_created stay only in
-- account_role_table and are never copied into a profile table.
-- ------------------------------------------------------------

-- STUDENT profile: one row per student account (UNIQUE account_id).
-- school_id is the school ID number and cannot be used twice.
CREATE TABLE IF NOT EXISTS `student_info_table` (
    `student_info_id` INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`    INT          NOT NULL,
    `school_id`     VARCHAR(30)  NOT NULL,
    `last_name`     VARCHAR(80)  NOT NULL,
    `first_name`    VARCHAR(80)  NOT NULL,
    `middle_initial` VARCHAR(10) NULL,
    `program_id`    INT          NOT NULL,
    `year_level`    VARCHAR(20)  NOT NULL,
    `institute_id`  INT          NOT NULL,
    `contact_number` VARCHAR(20) NOT NULL,
    `address`       VARCHAR(255) NOT NULL,
    UNIQUE KEY `ux_student_account` (`account_id`),
    UNIQUE KEY `ux_student_school_id` (`school_id`),
    CONSTRAINT `fk_student_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_student_institute`
        FOREIGN KEY (`institute_id`) REFERENCES `institute_table` (`institute_id`),
    CONSTRAINT `fk_student_program`
        FOREIGN KEY (`program_id`) REFERENCES `program_table` (`program_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- FACULTY profile: the institute is required. A faculty member is not asked for
-- a program, so no program_id is stored on this row.
CREATE TABLE IF NOT EXISTS `faculty_info_table` (
    `faculty_info_id` INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`   INT NOT NULL,
    `institute_id` INT NOT NULL,
    `contact_number` VARCHAR(20) NOT NULL DEFAULT '',
    UNIQUE KEY `ux_faculty_account` (`account_id`),
    CONSTRAINT `fk_faculty_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_faculty_institute`
        FOREIGN KEY (`institute_id`) REFERENCES `institute_table` (`institute_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- STAFF profile: one row per staff account. It links straight to the account
-- (account_table) and to its role row (account_role_table), so the office is
-- stored once and the role/status are never copied into this table.
-- NORMALISATION:
--   * account_id and role_id are both UNIQUE, so each is a candidate key and
--     every non-key column (office) depends on the whole key (no partial or
--     transitive dependency).
--   * fk_staff_account removes the account when it is deleted.
--   * fk_staff_role removes the profile when the role row is deleted.
CREATE TABLE IF NOT EXISTS `staff_info_table` (
    `staff_info_id` INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`    INT          NOT NULL,
    `role_id`       INT          NOT NULL,
    `office`        VARCHAR(120) NOT NULL,
    `contact_number` VARCHAR(20) NOT NULL DEFAULT '',
    UNIQUE KEY `ux_staff_account` (`account_id`),
    UNIQUE KEY `ux_staff_role` (`role_id`),
    CONSTRAINT `fk_staff_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_staff_role`
        FOREIGN KEY (`role_id`) REFERENCES `account_role_table` (`role_id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- LIBRARIAN profile: the librarian is the system admin, so the role gets its
-- own table with the same normalised shape as staff_info_table, but without an
-- office. account_id -> account_table, role_id -> account_role_table. The
-- contact number is editable from the PROFILE tab.
CREATE TABLE IF NOT EXISTS `librarian_info_table` (
    `librarian_info_id` INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`        INT          NOT NULL,
    `role_id`           INT          NOT NULL,
    `contact_number`    VARCHAR(20)  NOT NULL DEFAULT '',
    UNIQUE KEY `ux_librarian_account` (`account_id`),
    UNIQUE KEY `ux_librarian_role` (`role_id`),
    CONSTRAINT `fk_librarian_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_librarian_role`
        FOREIGN KEY (`role_id`) REFERENCES `account_role_table` (`role_id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- Older copies stored a program for faculty and an office for the librarian.
-- The app removes both automatically on the next sign in, or run this once:
-- ALTER TABLE `faculty_info_table` DROP FOREIGN KEY `fk_faculty_program`;
-- ALTER TABLE `faculty_info_table` DROP COLUMN `program_id`;
-- ALTER TABLE `librarian_info_table` DROP COLUMN `office`;

-- Run this once if the profile tables were created before the PROFILE tab
-- started collecting a contact number (the app also adds the columns
-- automatically through Authentication.EnsureContactNumberColumns):
-- ALTER TABLE `faculty_info_table` ADD COLUMN `contact_number` VARCHAR(20) NOT NULL DEFAULT '';
-- ALTER TABLE `staff_info_table`   ADD COLUMN `contact_number` VARCHAR(20) NOT NULL DEFAULT '';
-- ALTER TABLE `librarian_info_table` ADD COLUMN `contact_number` VARCHAR(20) NOT NULL DEFAULT '';

-- Run this once if staff_info_table was created before the role link existed:
-- ALTER TABLE `staff_info_table` ADD COLUMN `role_id` INT NULL AFTER `account_id`;
-- UPDATE `staff_info_table` st
-- JOIN `account_role_table` r ON r.`account_id` = st.`account_id`
-- SET st.`role_id` = r.`role_id`;
-- ALTER TABLE `staff_info_table` MODIFY `role_id` INT NOT NULL;
-- ALTER TABLE `staff_info_table` ADD UNIQUE KEY `ux_staff_role` (`role_id`);
-- ALTER TABLE `staff_info_table` ADD CONSTRAINT `fk_staff_role`
--     FOREIGN KEY (`role_id`) REFERENCES `account_role_table` (`role_id`)
--     ON DELETE CASCADE;

-- ------------------------------------------------------------
-- INSERT written by signup.cs (create_button_Click)
-- All of it runs in ONE transaction, so a failed profile insert
-- does not leave an account without a role.
-- name = the name shown in the dashboard:
--   STUDENT -> 'LASTNAME, FIRSTNAME M.I.'
--   LIBRARIAN / FACULTY / STAFF -> the typed full name
-- ------------------------------------------------------------
-- INSERT INTO `account_table` (`name`, `username`, `password`)
-- VALUES ('DITCHON, JOHNCEL VIC D.', 'juan', 'juan123');
-- INSERT INTO `account_role_table` (`account_id`, `role`, `status`, `date_created`)
-- VALUES (LAST_INSERT_ID(), 'STUDENT', 'PENDING', UPPER(DATE_FORMAT(NOW(), '%M %e, %Y')));

-- the program is reused when it already exists for that institute
-- INSERT INTO `program_table` (`program_name`, `institute_id`)
-- VALUES ('BS INFORMATION TECHNOLOGY', 1)
-- ON DUPLICATE KEY UPDATE `program_id` = LAST_INSERT_ID(`program_id`);

-- INSERT INTO `student_info_table` (`account_id`, `school_id`, `last_name`,
--                                   `first_name`, `middle_initial`, `program_id`,
--                                   `year_level`, `institute_id`,
--                                   `contact_number`, `address`)
-- VALUES (LAST_INSERT_ID(), '2026-00123', 'DITCHON', 'JOHNCEL VIC', 'D',
--         1, '1ST YEAR', 1, '0917 123 4567', 'PUROK 1, TANGUB CITY');

-- FACULTY version of the profile insert (no program for faculty)
-- INSERT INTO `faculty_info_table` (`account_id`, `institute_id`,
--                                   `contact_number`)
-- VALUES (LAST_INSERT_ID(), 1, '0917 123 4567');

-- STAFF version of the profile insert (written when the STAFF role is picked).
-- role_id is the id returned by the account_role_table insert above.
-- INSERT INTO `staff_info_table` (`account_id`, `role_id`, `office`, `contact_number`)
-- VALUES (LAST_INSERT_ID(), 1, 'REGISTRAR OFFICE', '0917 123 4567');

-- LIBRARIAN version of the profile insert (the system admin role, no office)
-- INSERT INTO `librarian_info_table` (`account_id`, `role_id`)
-- VALUES (LAST_INSERT_ID(), 1);

-- ------------------------------------------------------------
-- SELECT used by login.cs (login_button_Click)
-- ------------------------------------------------------------
-- SELECT a.`account_id`, a.`name`, a.`username`, r.`role`, r.`status`, r.`date_created`
-- FROM `account_table` a
-- LEFT JOIN `account_role_table` r ON r.`account_id` = a.`account_id`
-- WHERE a.`username` = 'juan' AND a.`password` = 'juan123'
-- LIMIT 1;

-- ------------------------------------------------------------
-- If date_created is still a TIMESTAMP, convert it once so the
-- app can save the readable text (login.cs does this automatically).
-- ------------------------------------------------------------
-- ALTER TABLE `account_role_table` MODIFY `date_created` VARCHAR(30) NOT NULL;
-- UPDATE `account_role_table`
-- SET `date_created` = UPPER(DATE_FORMAT(STR_TO_DATE(`date_created`, '%Y-%m-%d %H:%i:%s'), '%M %e, %Y'))
-- WHERE `date_created` REGEXP '^[0-9]{4}-[0-9]{2}-[0-9]{2}';

-- ------------------------------------------------------------
-- BOOTSTRAP: every new account starts as PENDING, so approve the
-- first LIBRARIAN / STAFF account once with this statement.
-- ------------------------------------------------------------
-- UPDATE `account_role_table` r
-- JOIN `account_table` a ON a.`account_id` = r.`account_id`
-- SET r.`status` = 'ACTIVE'
-- WHERE a.`username` = 'your_librarian_username';

-- CRUD checks
-- READ an account with its role and student profile
-- SELECT a.`account_id`, a.`name`, a.`username`, r.`role`, r.`status`, r.`date_created`,
--        s.`school_id`, p.`program_name`, s.`year_level`, i.`institute_code`
-- FROM `account_table` a
-- LEFT JOIN `account_role_table` r ON r.`account_id` = a.`account_id`
-- LEFT JOIN `student_info_table` s ON s.`account_id` = a.`account_id`
-- LEFT JOIN `program_table` p ON p.`program_id` = s.`program_id`
-- LEFT JOIN `institute_table` i ON i.`institute_id` = s.`institute_id`
-- ORDER BY a.`account_id`;
-- UPDATE (approve an account)
-- UPDATE `account_role_table` SET `status` = 'ACTIVE' WHERE `account_id` = 1;
-- DELETE (the role row and the profile row are removed automatically)
-- DELETE FROM `account_table` WHERE `account_id` = 1;

-- ============================================================
--  LIBRARY TABLES USED BY THE ADMIN DASHBOARD
--    books_table               (book_id, book_title, author, year_published,
--                               copy, date_added, book_image)
--    borrowers_table           (borrower_id, borrower_name, program,
--                               year_level, institute, id_number,
--                               day_before_borrow)
--    borrow_transactions_table (borrow_id, borrower_id, book_id,
--                               date_borrow, date_return)
--    borrow_requests_table     (request_id, account_id, book_id, copies_requested,
--                               request_date, status, note, processed_by,
--                               processed_date, borrow_id)
--
--  The dashboard reads and writes these tables directly:
--    ADD BOOK       -> INSERT INTO books_table
--    BORROW BOOK    -> INSERT INTO borrowers_table (new borrower)
--                      INSERT INTO borrow_transactions_table (the loan)
--    MARK RETURNED  -> UPDATE borrow_transactions_table.date_return
--    EDIT icon      -> UPDATE books_table / borrowers_table + borrow_transactions_table
--    DELETE icon    -> DELETE FROM books_table / borrow_transactions_table
--    STUDENT REQUEST -> INSERT INTO borrow_requests_table (PENDING)
--    APPROVE REQUEST -> normal BORROW BOOK flow + borrow_id on the request
--    REJECT / CANCEL -> UPDATE borrow_requests_table.status
--
--  A title stays AVAILABLE while at least one copy is not in an open
--  borrow_transactions_table row (date_return IS NULL). AVAILABLE COPY is
--  books_table.copy minus the number of open loans for that title.
--
--  No due date is stored or computed. A loan is measured by DAYS BORROWED,
--  and the day is only added after 24 full hours: a book borrowed on
--  OCT 28, 2026 1:15 AM shows 0 days until OCT 29, 2026 1:15 AM, which is
--  day 1. borrowers_table.day_before_borrow keeps that whole number, and
--  the dashboard rewrites it from date_borrow on every load, so the stored
--  column can never drift away from the clock.
--
--  ACCOUNT MANAGEMENT (the four account tabs)
--    PROFILE            -> the signed in row, joined to student_info_table /
--                          faculty_info_table / staff_info_table for program
--                          (student only), institute, year level, school id,
--                          contact number and office (staff only)
--    LIBRARIAN ACCOUNTS -> LIBRARIAN (plus the legacy ADMIN role)
--    STAFF ACCOUNTS     -> STAFF (plus the legacy OFFICER role)
--    FACULTY ACCOUNTS   -> account_role_table.role = 'FACULTY'
--    STUDENT ACCOUNT    -> account_role_table.role = 'STUDENT'
--    status = 'PENDING'  -> waiting for approval, cannot sign in
--    status = 'ACTIVE'   -> may sign in
--    status = 'INACTIVE' -> deactivated by the staff, cannot sign in
--  Only LIBRARIAN / STAFF logins may change it (or the built in 'admin').
-- ============================================================

CREATE TABLE IF NOT EXISTS `books_table` (
    `book_id`        INT(11)      NOT NULL AUTO_INCREMENT,
    `book_title`     VARCHAR(255) NOT NULL,
    `author`         VARCHAR(150) DEFAULT NULL,
    `year_published` SMALLINT(6)  DEFAULT NULL,
    `copy`           VARCHAR(250) NOT NULL,
    `date_added`     VARCHAR(250) NOT NULL,
    `book_image`     MEDIUMBLOB   DEFAULT NULL,
    PRIMARY KEY (`book_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

CREATE TABLE IF NOT EXISTS `borrowers_table` (
    `borrower_id`       INT(11)      NOT NULL AUTO_INCREMENT,
    `borrower_name`     VARCHAR(150) NOT NULL,
    `program`           VARCHAR(100) DEFAULT NULL,
    `year_level`        VARCHAR(50)  DEFAULT NULL,
    `institute`         VARCHAR(100) DEFAULT NULL,
    `id_number`         VARCHAR(50)  NOT NULL,
    `day_before_borrow` INT(11)      NOT NULL DEFAULT 0,
    PRIMARY KEY (`borrower_id`),
    UNIQUE KEY `id_number` (`id_number`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

CREATE TABLE IF NOT EXISTS `borrow_transactions_table` (
    `borrow_id`   INT(11)  NOT NULL AUTO_INCREMENT,
    `borrower_id` INT(11)  NOT NULL,
    `book_id`     INT(11)  NOT NULL,
    `date_borrow` DATETIME NOT NULL,
    `date_return` DATETIME DEFAULT NULL,
    PRIMARY KEY (`borrow_id`),
    KEY `fk_borrower` (`borrower_id`),
    KEY `fk_book` (`book_id`),
    CONSTRAINT `fk_book` FOREIGN KEY (`book_id`) REFERENCES `books_table` (`book_id`) ON UPDATE CASCADE,
    CONSTRAINT `fk_borrower` FOREIGN KEY (`borrower_id`) REFERENCES `borrowers_table` (`borrower_id`) ON UPDATE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- STUDENT REQUEST QUEUE
-- A student submits a PENDING row from the student dashboard. The librarian
-- approves it in BORROW REQUESTS, which writes the normal borrow transaction
-- and links the request to that borrow_id. Rejected and cancelled requests
-- stay on file for the audit trail.
CREATE TABLE IF NOT EXISTS `borrow_requests_table` (
    `request_id`     INT(11)      NOT NULL AUTO_INCREMENT,
    `account_id`     INT(11)      NOT NULL,
    `book_id`        INT(11)      NOT NULL,
    `copies_requested` INT(11)    NOT NULL DEFAULT 1,
    `request_date`   DATETIME     NOT NULL,
    `status`         VARCHAR(20)  NOT NULL DEFAULT 'PENDING',
    `note`           VARCHAR(255) DEFAULT NULL,
    `processed_by`   INT(11)      DEFAULT NULL,
    `processed_date` DATETIME     DEFAULT NULL,
    `borrow_id`      INT(11)      DEFAULT NULL,
    PRIMARY KEY (`request_id`),
    KEY `idx_request_account` (`account_id`),
    KEY `idx_request_status` (`status`),
    KEY `idx_request_book` (`book_id`),
    CONSTRAINT `fk_request_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_request_book`
        FOREIGN KEY (`book_id`) REFERENCES `books_table` (`book_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_request_processor`
        FOREIGN KEY (`processed_by`) REFERENCES `account_table` (`account_id`)
        ON DELETE SET NULL,
    CONSTRAINT `fk_request_borrow`
        FOREIGN KEY (`borrow_id`) REFERENCES `borrow_transactions_table` (`borrow_id`)
        ON DELETE SET NULL
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- LIBRARY OFFICE ATTENDANCE
-- One row per accepted QR scan. The scanner reads the ACCOUNT line of the pass,
-- checks that the account exists and is ACTIVE, then writes the visit with the
-- server clock. The account details shown in MY ATTENDANCE / LIBRARY ATTENDANCE
-- are read back through the account foreign key, so they never go out of date.
CREATE TABLE IF NOT EXISTS `attendance_table` (
    `attendance_id` INT(11)     NOT NULL AUTO_INCREMENT,
    `account_id`    INT(11)     NOT NULL,
    `scan_time`     DATETIME    NOT NULL,
    `role`          VARCHAR(20) NOT NULL,
    PRIMARY KEY (`attendance_id`),
    KEY `idx_attendance_account` (`account_id`),
    KEY `idx_attendance_time` (`scan_time`),
    CONSTRAINT `fk_attendance_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- LIBRARY EVALUATION FORM
-- A public survey opened from the welcome page. One evaluation_table row holds
-- the submission (role, optional name and time) and one evaluation_answer_table
-- row holds each 1-5 rating, so answers stay normalised and can be averaged by
-- section or by question. The form creates these tables itself on first submit.
CREATE TABLE IF NOT EXISTS `evaluation_table` (
    `evaluation_id`   INT(11)      NOT NULL AUTO_INCREMENT,
    `account_id`      INT(11)      DEFAULT NULL,
    `respondent_role` VARCHAR(20)  NOT NULL,
    `respondent_name` VARCHAR(150) NOT NULL DEFAULT '',
    `submitted_at`    DATETIME     NOT NULL,
    PRIMARY KEY (`evaluation_id`),
    KEY `idx_evaluation_account` (`account_id`),
    CONSTRAINT `fk_evaluation_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE SET NULL
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

CREATE TABLE IF NOT EXISTS `evaluation_answer_table` (
    `answer_id`     INT(11)  NOT NULL AUTO_INCREMENT,
    `evaluation_id` INT(11)  NOT NULL,
    `section_no`    INT(11)  NOT NULL,
    `question_no`   INT(11)  NOT NULL,
    `rating`        TINYINT  NOT NULL,
    PRIMARY KEY (`answer_id`),
    KEY `idx_answer_evaluation` (`evaluation_id`),
    CONSTRAINT `fk_answer_evaluation`
        FOREIGN KEY (`evaluation_id`) REFERENCES `evaluation_table` (`evaluation_id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- Older copies of books_table declared year_published as YEAR(4), which cannot
-- hold titles such as Noli Me Tangere (1887). Run this once if that is your copy:
-- ALTER TABLE `books_table` MODIFY `year_published` SMALLINT NULL;

-- Run this once on an older database to store book covers:
-- ALTER TABLE `books_table` ADD COLUMN `book_image` MEDIUMBLOB NULL AFTER `date_added`;

-- The copies column is called `copy` since the EDIT BOOK pop up was added. Run
-- this once on an older database so books_table matches the app queries:
-- ALTER TABLE `books_table` CHANGE `quantity` `copy` VARCHAR(250) NOT NULL;

-- Older copies stored the borrow dates without a time of day and wrote text in
-- day_before_borrow. Run these once so the 24 hour counter has a real clock:
-- UPDATE `borrowers_table` SET `day_before_borrow` = '0'
--     WHERE `day_before_borrow` = '' OR `day_before_borrow` IS NULL;
-- ALTER TABLE `borrowers_table` MODIFY `day_before_borrow` INT(11) NOT NULL DEFAULT 0;
-- ALTER TABLE `borrow_transactions_table`
--     MODIFY `date_borrow` DATETIME NOT NULL,
--     MODIFY `date_return` DATETIME DEFAULT NULL;

-- INSERT written by the ADD BOOK pop up
-- INSERT INTO `books_table` (`book_title`, `author`, `year_published`, `copy`, `date_added`)
-- VALUES ('Noli Me Tangere', 'Jose Rizal', 1887, '4', 'OCTOBER 28, 2026');

-- INSERT written by the BORROW BOOK pop up
-- INSERT INTO `borrowers_table` (`borrower_name`, `program`, `year_level`, `institute`, `id_number`, `day_before_borrow`)
-- VALUES ('Juan Dela Cruz', 'BSIT', '3RD YEAR', 'INSTITUTE OF COMPUTING STUDIES', '2026-00123', 0);
-- INSERT INTO `borrow_transactions_table` (`borrower_id`, `book_id`, `date_borrow`, `date_return`)
-- VALUES (LAST_INSERT_ID(), 1, '2026-10-28 01:15:00', NULL);

-- UPDATE written by MARK RETURNED (the clock time is stored too)
-- UPDATE `borrow_transactions_table` SET `date_return` = '2026-11-02 09:40:00' WHERE `borrow_id` = 1;

-- UPDATE written by the dashboard when it loads the loans (SyncDaysBorrowed)
-- UPDATE `borrowers_table` SET `day_before_borrow` = 1 WHERE `borrower_id` = 5;

-- REPORT queries used by GENERATE REPORT
-- SELECT * FROM `books_table` ORDER BY `book_title`;
-- SELECT t.`borrow_id`, b.`borrower_name`, b.`id_number`, bk.`book_title`, bk.`author`,
--        t.`date_borrow`, t.`date_return`
-- FROM `borrow_transactions_table` t
-- JOIN `borrowers_table` b ON b.`borrower_id` = t.`borrower_id`
-- LEFT JOIN `books_table` bk ON bk.`book_id` = t.`book_id`
-- ORDER BY t.`date_borrow` DESC;

-- UPDATE written by the ACTIVATE / DEACTIVATE buttons
-- UPDATE `account_role_table` SET `status` = 'ACTIVE'   WHERE `account_id` = 2;
-- UPDATE `account_role_table` SET `status` = 'INACTIVE' WHERE `account_id` = 2;

-- ============================================================
--  EDIT / DELETE ICONS OF THE DASHBOARD TABLES
--
--  Every table (LIBRARIAN ACCOUNTS, STAFF ACCOUNTS, BOOKS, BORROWED
--  BOOKS, FACULTY ACCOUNTS, STUDENT ACCOUNTS) has one EDIT and one
--  DELETE icon column. The EDIT icon opens a pop up that rewrites the
--  same row; the DELETE icon removes it after a confirmation.
-- ============================================================

-- EDIT BOOK (books_table)
-- UPDATE `books_table`
-- SET `book_title` = 'Noli Me Tangere', `author` = 'Jose Rizal',
--     `year_published` = 1887, `copy` = '4'
-- WHERE `book_id` = 1;

-- DELETE BOOK (only when the title has no borrow rows yet)
-- DELETE FROM `books_table` WHERE `book_id` = 1;

-- EDIT BORROW RECORD (borrowers_table + borrow_transactions_table)
-- UPDATE `borrowers_table`
-- SET `borrower_name` = 'Juan Dela Cruz', `program` = 'BSIT',
--     `year_level` = '3RD YEAR', `institute` = 'ICS', `id_number` = '2026-00123'
-- WHERE `borrower_id` = 5;
-- UPDATE `borrow_transactions_table`
-- SET `book_id` = 1, `date_borrow` = '2026-10-28 01:15:00',
--     `date_return` = NULL
-- WHERE `borrow_id` = 1;

-- DELETE BORROW RECORD (the book becomes available again)
-- DELETE FROM `borrow_transactions_table` WHERE `borrow_id` = 1;

-- EDIT ACCOUNT (account_table + account_role_table)
-- UPDATE `account_table` SET `name` = 'DITCHON, JOHNCEL VIC D.',
--     `username` = 'juan' WHERE `account_id` = 2;
-- UPDATE `account_role_table` SET `role` = 'STUDENT', `status` = 'ACTIVE'
-- WHERE `account_id` = 2;

-- EDIT MY INFO (self service PROFILE tab of both dashboards)
-- The signed in person writes the account name / username and every profile
-- value that belongs to the role (school id, program, year level, institute,
-- contact number, address, office). The role, the status and the date created
-- stay read only, so a patron cannot promote itself.
-- UPDATE `account_table` SET `name` = '...', `username` = '...'
-- WHERE `account_id` = 2;
-- UPDATE `student_info_table`
-- SET `school_id` = '...', `program_id` = 1, `year_level` = '3RD YEAR',
--     `institute_id` = 1, `contact_number` = '...', `address` = '...'
-- WHERE `account_id` = 2;
-- INSERT INTO `faculty_info_table` (`account_id`, `institute_id`, `contact_number`)
-- VALUES (3, 1, '...')
-- ON DUPLICATE KEY UPDATE `institute_id` = 1, `contact_number` = '...';
-- UPDATE `staff_info_table` SET `office` = '...', `contact_number` = '...'
-- WHERE `account_id` = 4;
-- UPDATE `librarian_info_table` SET `contact_number` = '...' WHERE `account_id` = 5;

-- EDIT STUDENT PROFILE (student_info_table; program_id comes from
-- program_table through ResolveProgramId, institute_id from institute_table)
-- UPDATE `student_info_table`
-- SET `school_id` = '2026-00123', `program_id` = 1, `year_level` = '1ST YEAR',
--     `institute_id` = 1, `contact_number` = '0917 123 4567'
-- WHERE `account_id` = 2;

-- EDIT FACULTY PROFILE (faculty_info_table; no program for faculty)
-- UPDATE `faculty_info_table` SET `institute_id` = 1
-- WHERE `account_id` = 3;

-- EDIT STAFF PROFILE (staff_info_table; role_id is the account's role row)
-- INSERT INTO `staff_info_table` (`account_id`, `role_id`, `office`)
-- VALUES (4, 4, 'REGISTRAR OFFICE')
-- ON DUPLICATE KEY UPDATE `role_id` = 4, `office` = 'REGISTRAR OFFICE';

-- EDIT LIBRARIAN PROFILE (librarian_info_table; no office for the librarian)
-- INSERT INTO `librarian_info_table` (`account_id`, `role_id`)
-- VALUES (5, 5)
-- ON DUPLICATE KEY UPDATE `role_id` = 5;

-- DELETE ACCOUNT (role and profile rows cascade with it)
-- DELETE FROM `account_table` WHERE `account_id` = 2;


