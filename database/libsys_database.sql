-- ============================================================
--  TCGC Library Management System
--  THIRD NORMAL FORM (3NF) SCHEMA
--
--  Every table below has:
--    * a surrogate primary key,
--    * only atomic (single valued) columns,
--    * no repeating group,
--    * no non-key attribute that depends on another non-key attribute.
--
--  Repeated text values (role, status) live in lookup tables and are
--  referenced by a foreign key. Values that can be derived from another
--  table are NOT stored twice:
--    * a student's institute comes from program_table,
--    * the borrower's display name comes from last_name / first_name,
--    * the days borrowed comes from the two loan dates,
--    * a loan's BORROWED / RETURNED state comes from date_return,
--    * attendance role comes from account_role_table.
--
--  Table map
--    role_table                  role_id, role_code
--    account_status_table        status_id, status_code
--    request_status_table        request_status_id, status_code
--    institute_table             institute_id, institute_code, institute_name
--    program_table               program_id, program_name, institute_id
--    account_table               account_id, name, username, password, profile_picture
--    account_role_table          account_role_id, account_id, role_id, status_id, date_created
--    student_info_table          student_info_id, account_id, school_id, names, program_id, year_level, contact, address
--    faculty_info_table          faculty_info_id, account_id, institute_id, contact_number
--    staff_info_table            staff_info_id, account_id, office, contact_number
--    librarian_info_table        librarian_info_id, account_id, contact_number
--    books_table                 book_id, book_title, author, year_published, copies, date_added, book_image
--    borrowers_table             borrower_id, account_id, names, id_number, contact_number, role_id,
--                                program_id, year_level, institute_id, office
--    borrow_transactions_table   borrow_id, borrower_id, book_id, date_borrow, date_return
--    borrow_requests_table       request_id, account_id, book_id, copies_requested, request_date,
--                                status_id, processed_by, processed_date, claimed_by, claim_date, borrow_id
--    attendance_table            attendance_id, account_id, scan_time
--    evaluation_period_table     period_id, semester, school_year, opened_at, closed_at, is_open (derived)
--    evaluation_question_table   question_id, section_no, question_no, question_text
--    evaluation_table            evaluation_id, account_id, period_id, submitted_at
--    evaluation_respondent_table evaluation_id, role_id, respondent_name
--    evaluation_answer_table     answer_id, evaluation_id, question_id, rating
-- ============================================================

CREATE DATABASE IF NOT EXISTS `libsys_database`
    DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

USE `libsys_database`;

-- ------------------------------------------------------------
-- LOOKUP TABLES
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `role_table` (
    `role_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `role_code` VARCHAR(20) NOT NULL,
    UNIQUE KEY `ux_role_code` (`role_code`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

INSERT IGNORE INTO `role_table` (`role_code`) VALUES
    ('STUDENT'),
    ('FACULTY'),
    ('STAFF'),
    ('LIBRARIAN');

CREATE TABLE IF NOT EXISTS `account_status_table` (
    `status_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `status_code` VARCHAR(20) NOT NULL,
    UNIQUE KEY `ux_account_status_code` (`status_code`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

INSERT IGNORE INTO `account_status_table` (`status_code`) VALUES
    ('PENDING'),
    ('ACTIVE'),
    ('INACTIVE'),
    ('REJECTED');

CREATE TABLE IF NOT EXISTS `request_status_table` (
    `request_status_id` INT AUTO_INCREMENT PRIMARY KEY,
    `status_code`       VARCHAR(20) NOT NULL,
    UNIQUE KEY `ux_request_status_code` (`status_code`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

INSERT IGNORE INTO `request_status_table` (`status_code`) VALUES
    ('PENDING'),
    ('APPROVED'),
    ('REJECTED'),
    ('CANCELLED'),
    ('CLAIMED');

-- ------------------------------------------------------------
-- SCHOOL LOOKUPS
-- A program belongs to exactly one institute, so a student row never
-- stores institute_id: it is read through program_table.
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `institute_table` (
    `institute_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `institute_code` VARCHAR(10)  NOT NULL,
    `institute_name` VARCHAR(120) NOT NULL,
    UNIQUE KEY `ux_institute_code` (`institute_code`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

INSERT IGNORE INTO `institute_table` (`institute_code`, `institute_name`) VALUES
    ('ICS',  'INSTITUTE OF COMPUTER STUDIES'),
    ('IAS',  'INSTITUTE OF ARTS AND SCIENCES'),
    ('ICJE', 'INSTITUTE OF CRIMINAL JUSTICE EDUCATION'),
    ('IBFS', 'INSTITUTE OF BUSINESS AND FINANCE STUDIES'),
    ('IHS',  'INSTITUTE OF HEALTH SCIENCES'),
    ('ITE',  'INSTITUTE OF TEACHER EDUCATION');

CREATE TABLE IF NOT EXISTS `program_table` (
    `program_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `program_name` VARCHAR(120) NOT NULL,
    `institute_id` INT          NOT NULL,
    UNIQUE KEY `ux_program_name_institute` (`program_name`, `institute_id`),
    CONSTRAINT `fk_program_institute`
        FOREIGN KEY (`institute_id`) REFERENCES `institute_table` (`institute_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

-- ------------------------------------------------------------
-- ACCOUNT
-- `name` is the single display name of the account. It is not copied
-- into any other table.
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `account_table` (
    `account_id`      INT AUTO_INCREMENT PRIMARY KEY,
    `name`            VARCHAR(250) NOT NULL,
    `username`        VARCHAR(250) NOT NULL,
    `password`        VARCHAR(255) NOT NULL,
    `profile_picture` MEDIUMBLOB   NULL,
    UNIQUE KEY `ux_account_username` (`username`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

-- One role row per account. The role and the status are not text on this
-- row: they are foreign keys to role_table and account_status_table.
CREATE TABLE IF NOT EXISTS `account_role_table` (
    `account_role_id` INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`      INT  NOT NULL,
    `role_id`         INT  NOT NULL,
    `status_id`       INT  NOT NULL,
    `date_created`    DATE NOT NULL,
    UNIQUE KEY `ux_account_role_account` (`account_id`),
    CONSTRAINT `fk_account_role_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_account_role_role`
        FOREIGN KEY (`role_id`) REFERENCES `role_table` (`role_id`),
    CONSTRAINT `fk_account_role_status`
        FOREIGN KEY (`status_id`) REFERENCES `account_status_table` (`status_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

-- ------------------------------------------------------------
-- ROLE PROFILES
-- Each profile is a 1:1 extension of account_table. A profile never
-- repeats the role: the role is read from account_role_table through
-- account_id.
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `student_info_table` (
    `student_info_id` INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`      INT         NOT NULL,
    `school_id`       VARCHAR(30) NOT NULL,
    `last_name`       VARCHAR(80) NOT NULL,
    `first_name`      VARCHAR(80) NOT NULL,
    `middle_initial`  VARCHAR(10) NULL,
    `program_id`      INT         NOT NULL,
    `year_level`      VARCHAR(20) NOT NULL,
    `contact_number`  VARCHAR(20) NOT NULL,
    `address`         VARCHAR(255) NOT NULL,
    UNIQUE KEY `ux_student_account` (`account_id`),
    UNIQUE KEY `ux_student_school_id` (`school_id`),
    CONSTRAINT `fk_student_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_student_program`
        FOREIGN KEY (`program_id`) REFERENCES `program_table` (`program_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `faculty_info_table` (
    `faculty_info_id` INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`      INT NOT NULL,
    `institute_id`    INT NOT NULL,
    `contact_number`  VARCHAR(20) NOT NULL DEFAULT '',
    UNIQUE KEY `ux_faculty_account` (`account_id`),
    CONSTRAINT `fk_faculty_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_faculty_institute`
        FOREIGN KEY (`institute_id`) REFERENCES `institute_table` (`institute_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `staff_info_table` (
    `staff_info_id`  INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`     INT          NOT NULL,
    `office`         VARCHAR(120) NOT NULL,
    `contact_number` VARCHAR(20)  NOT NULL DEFAULT '',
    UNIQUE KEY `ux_staff_account` (`account_id`),
    CONSTRAINT `fk_staff_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `librarian_info_table` (
    `librarian_info_id` INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`        INT NOT NULL,
    `contact_number`    VARCHAR(20) NOT NULL DEFAULT '',
    UNIQUE KEY `ux_librarian_account` (`account_id`),
    CONSTRAINT `fk_librarian_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

-- ------------------------------------------------------------
-- CATALOG
-- `copies` is a whole number and `date_added` is a real DATE. The
-- AVAILABLE / BORROWED state is never stored: it is worked out from
-- the open rows of borrow_transactions_table.
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `books_table` (
    `book_id`        INT AUTO_INCREMENT PRIMARY KEY,
    `book_title`     VARCHAR(255) NOT NULL,
    `author`         VARCHAR(150) NULL,
    `year_published` SMALLINT     NULL,
    `copies`         INT          NOT NULL,
    `date_added`     DATE         NOT NULL,
    `book_image`     MEDIUMBLOB   NULL,
    CONSTRAINT `chk_books_copies` CHECK (`copies` >= 1),
    CONSTRAINT `chk_books_year` CHECK (`year_published` IS NULL OR `year_published` >= 0)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

-- ------------------------------------------------------------
-- BORROWER (library patron)
-- The name is stored once, in its own columns. The program is a
-- foreign key. A student's institute is derived from program_table;
-- institute_id is only used for a faculty borrower who has no
-- program row, so the institute is never a copy of another column.
-- day_before_borrow is NOT stored: it is calculated from date_borrow.
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `borrowers_table` (
    `borrower_id`    INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`     INT          NULL,
    `last_name`      VARCHAR(80)  NOT NULL,
    `first_name`     VARCHAR(80)  NOT NULL DEFAULT '',
    `middle_name`    VARCHAR(50)  NULL,
    `id_number`      VARCHAR(50)  NULL,
    `contact_number` VARCHAR(30)  NOT NULL DEFAULT '',
    `role_id`        INT          NOT NULL,
    `program_id`     INT          NULL,
    `year_level`     VARCHAR(50)  NULL,
    `institute_id`   INT          NULL,
    `office`         VARCHAR(100) NULL,
    UNIQUE KEY `ux_borrower_account` (`account_id`),
    UNIQUE KEY `ux_borrower_id_number` (`id_number`),
    CONSTRAINT `fk_borrower_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE SET NULL,
    CONSTRAINT `fk_borrower_role`
        FOREIGN KEY (`role_id`) REFERENCES `role_table` (`role_id`),
    CONSTRAINT `fk_borrower_program`
        FOREIGN KEY (`program_id`) REFERENCES `program_table` (`program_id`),
    CONSTRAINT `fk_borrower_institute`
        FOREIGN KEY (`institute_id`) REFERENCES `institute_table` (`institute_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

-- One row per borrowed copy. BORROWED / RETURNED is derived from
-- date_return, and the loan length is derived from the two dates.
CREATE TABLE IF NOT EXISTS `borrow_transactions_table` (
    `borrow_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `borrower_id` INT      NOT NULL,
    `book_id`     INT      NOT NULL,
    `date_borrow` DATETIME NOT NULL,
    `date_return` DATETIME NULL,
    KEY `idx_borrow_borrower` (`borrower_id`),
    KEY `idx_borrow_book` (`book_id`),
    KEY `idx_borrow_return` (`date_return`),
    CONSTRAINT `fk_borrow_borrower`
        FOREIGN KEY (`borrower_id`) REFERENCES `borrowers_table` (`borrower_id`)
        ON UPDATE CASCADE,
    CONSTRAINT `fk_borrow_book`
        FOREIGN KEY (`book_id`) REFERENCES `books_table` (`book_id`)
        ON UPDATE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

-- ------------------------------------------------------------
-- STUDENT REQUEST QUEUE
-- The status is a foreign key, not free text. Every account link is
-- a foreign key as well.
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `borrow_requests_table` (
    `request_id`       INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`       INT      NOT NULL,
    `book_id`          INT      NOT NULL,
    `copies_requested` INT      NOT NULL DEFAULT 1,
    `request_date`     DATETIME NOT NULL,
    `status_id`        INT      NOT NULL,
    `processed_by`     INT      NULL,
    `processed_date`   DATETIME NULL,
    `claimed_by`       INT      NULL,
    `claim_date`       DATETIME NULL,
    `borrow_id`        INT      NULL,
    KEY `idx_request_account` (`account_id`),
    KEY `idx_request_status` (`status_id`),
    KEY `idx_request_book` (`book_id`),
    KEY `idx_request_processor` (`processed_by`),
    KEY `idx_request_claimant` (`claimed_by`),
    KEY `idx_request_borrow` (`borrow_id`),
    CONSTRAINT `fk_request_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_request_book`
        FOREIGN KEY (`book_id`) REFERENCES `books_table` (`book_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_request_status`
        FOREIGN KEY (`status_id`) REFERENCES `request_status_table` (`request_status_id`),
    CONSTRAINT `fk_request_processor`
        FOREIGN KEY (`processed_by`) REFERENCES `account_table` (`account_id`)
        ON DELETE SET NULL,
    CONSTRAINT `fk_request_claimant`
        FOREIGN KEY (`claimed_by`) REFERENCES `account_table` (`account_id`)
        ON DELETE SET NULL,
    CONSTRAINT `fk_request_borrow`
        FOREIGN KEY (`borrow_id`) REFERENCES `borrow_transactions_table` (`borrow_id`)
        ON DELETE SET NULL,
    CONSTRAINT `chk_request_copies` CHECK (`copies_requested` >= 1)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

-- ------------------------------------------------------------
-- LIBRARY OFFICE ATTENDANCE
-- No role column: the role is read from account_role_table through
-- account_id, so it can never go stale.
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `attendance_table` (
    `attendance_id` INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`    INT      NOT NULL,
    `scan_time`     DATETIME NOT NULL,
    KEY `idx_attendance_account` (`account_id`),
    KEY `idx_attendance_time` (`scan_time`),
    CONSTRAINT `fk_attendance_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

-- ------------------------------------------------------------
-- LIBRARY EVALUATION
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `evaluation_period_table` (
    `period_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `semester`    VARCHAR(40) NOT NULL,
    `school_year` VARCHAR(20) NOT NULL,
    `opened_at`   DATETIME    NULL,
    `closed_at`   DATETIME    NULL,
    `is_open`     TINYINT(1)  AS (CASE WHEN `opened_at` IS NOT NULL AND `closed_at` IS NULL THEN 1 ELSE 0 END) STORED,
    `open_marker` TINYINT     AS (CASE WHEN `opened_at` IS NOT NULL AND `closed_at` IS NULL THEN 1 ELSE NULL END) STORED,
    UNIQUE KEY `uq_evaluation_period` (`semester`, `school_year`),
    UNIQUE KEY `ux_single_open_period` (`open_marker`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

-- The 25 statements are reference data, so they are stored once here
-- instead of repeating section_no / question_no on every answer.
CREATE TABLE IF NOT EXISTS `evaluation_question_table` (
    `question_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `section_no`    INT          NOT NULL,
    `question_no`   INT          NOT NULL,
    `question_text` VARCHAR(255) NOT NULL,
    UNIQUE KEY `ux_evaluation_question` (`section_no`, `question_no`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `evaluation_table` (
    `evaluation_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`      INT          NULL,
    `period_id`       INT          NOT NULL,
    `submitted_at`    DATETIME     NOT NULL,
    UNIQUE KEY `uq_evaluation_period_account` (`period_id`, `account_id`),
    KEY `idx_evaluation_account` (`account_id`),
    KEY `idx_evaluation_period` (`period_id`),
    CONSTRAINT `fk_evaluation_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE SET NULL,
    CONSTRAINT `fk_evaluation_period`
        FOREIGN KEY (`period_id`) REFERENCES `evaluation_period_table` (`period_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

-- The role and name are a respondent snapshot, so they live in their own
-- relation keyed by the evaluation instead of repeating on the submission row.
CREATE TABLE IF NOT EXISTS `evaluation_respondent_table` (
    `evaluation_id`   INT          PRIMARY KEY,
    `role_id`         INT          NOT NULL,
    `respondent_name` VARCHAR(150) NOT NULL DEFAULT '',
    KEY `idx_eval_respondent_role` (`role_id`),
    CONSTRAINT `fk_eval_respondent_evaluation`
        FOREIGN KEY (`evaluation_id`) REFERENCES `evaluation_table` (`evaluation_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_eval_respondent_role`
        FOREIGN KEY (`role_id`) REFERENCES `role_table` (`role_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `evaluation_answer_table` (
    `answer_id`     INT AUTO_INCREMENT PRIMARY KEY,
    `evaluation_id` INT     NOT NULL,
    `question_id`   INT     NOT NULL,
    `rating`        TINYINT NOT NULL,
    UNIQUE KEY `ux_answer_evaluation_question` (`evaluation_id`, `question_id`),
    KEY `idx_answer_evaluation` (`evaluation_id`),
    KEY `idx_answer_question` (`question_id`),
    CONSTRAINT `fk_answer_evaluation`
        FOREIGN KEY (`evaluation_id`) REFERENCES `evaluation_table` (`evaluation_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_answer_question`
        FOREIGN KEY (`question_id`) REFERENCES `evaluation_question_table` (`question_id`),
    CONSTRAINT `chk_answer_rating` CHECK (`rating` BETWEEN 1 AND 5)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

INSERT IGNORE INTO `evaluation_question_table` (`question_id`, `section_no`, `question_no`, `question_text`) VALUES
    (1,  1, 1, 'THE LIBRARY RESOURCE CENTER MAINTAINS A CLEAN, ORDERLY, AND WELL-MAINTAINED ENVIRONMENT.'),
    (2,  1, 2, 'THE LIBRARY PROVIDES ADEQUATE SEATING CAPACITY TO ACCOMMODATE ITS USERS.'),
    (3,  1, 3, 'THE LIBRARY ENSURES SUFFICIENT LIGHTING AND PROPER VENTILATION WITHIN ITS PREMISES.'),
    (4,  1, 4, 'THE LIBRARY PROVIDES A COMFORTABLE AND CONDUCIVE ENVIRONMENT FOR ACADEMIC STUDY AND RESEARCH.'),
    (5,  1, 5, 'THE LIBRARY FACILITIES ARE PROPERLY ARRANGED, ORGANIZED, AND MAINTAINED TO ENSURE USER CONVENIENCE.'),
    (6,  2, 1, 'THE LIBRARY PROVIDES AN ADEQUATE COLLECTION OF BOOKS AND REFERENCE MATERIALS TO SUPPORT ACADEMIC ACTIVITIES.'),
    (7,  2, 2, 'THE AVAILABLE BOOKS AND REFERENCE MATERIALS ARE RELEVANT TO THE ACADEMIC NEEDS OF STUDENTS.'),
    (8,  2, 3, 'LIBRARY RESOURCES ARE SYSTEMATICALLY ORGANIZED AND READILY ACCESSIBLE TO USERS.'),
    (9,  2, 4, 'THE LIBRARY PROVIDES UPDATED AND APPROPRIATE LEARNING MATERIALS THAT SUPPORT THE ACADEMIC CURRICULUM.'),
    (10, 2, 5, 'THE AVAILABLE LIBRARY RESOURCES EFFECTIVELY SUPPORT STUDENTS'' LEARNING, RESEARCH, AND ACADEMIC DEVELOPMENT.'),
    (11, 3, 1, 'LIBRARY PERSONNEL DEMONSTRATE PROFESSIONALISM, COURTESY, AND APPROACHABILITY WHEN ASSISTING USERS.'),
    (12, 3, 2, 'LIBRARY PERSONNEL PROVIDE TIMELY AND APPROPRIATE ASSISTANCE IN ADDRESSING USERS'' INQUIRIES AND CONCERNS.'),
    (13, 3, 3, 'THE PROCEDURES FOR BORROWING AND RETURNING LIBRARY MATERIALS ARE CONVENIENT AND EFFICIENTLY IMPLEMENTED.'),
    (14, 3, 4, 'LIBRARY SERVICES ARE DELIVERED EFFICIENTLY AND IN ACCORDANCE WITH ESTABLISHED PROCEDURES.'),
    (15, 3, 5, 'LIBRARY RULES, REGULATIONS, AND POLICIES ARE CLEARLY COMMUNICATED AND PROPERLY IMPLEMENTED.'),
    (16, 4, 1, 'THE LIBRARY RESOURCE CENTER IS READILY ACCESSIBLE TO STUDENTS AND OTHER AUTHORIZED USERS.'),
    (17, 4, 2, 'LIBRARY RESOURCES ARE SUFFICIENTLY AVAILABLE TO MEET THE ACADEMIC AND INFORMATIONAL NEEDS OF USERS.'),
    (18, 4, 3, 'THE LIBRARY MAINTAINS OPERATING HOURS THAT ADEQUATELY ACCOMMODATE THE SCHEDULES OF ITS USERS.'),
    (19, 4, 4, 'USERS CAN CONVENIENTLY LOCATE AND ACCESS THE BOOKS AND REFERENCE MATERIALS THEY REQUIRE.'),
    (20, 4, 5, 'LIBRARY SERVICES ARE CONSISTENTLY ACCESSIBLE DURING DESIGNATED OPERATING HOURS.'),
    (21, 5, 1, 'I AM SATISFIED WITH THE OVERALL CONDITION, ORGANIZATION, AND MAINTENANCE OF THE LIBRARY RESOURCE CENTER.'),
    (22, 5, 2, 'THE LIBRARY RESOURCE CENTER EFFECTIVELY MEETS MY ACADEMIC AND INFORMATIONAL NEEDS.'),
    (23, 5, 3, 'THE LIBRARY PROVIDES AN ENVIRONMENT THAT PROMOTES EFFECTIVE LEARNING, STUDYING, AND RESEARCH.'),
    (24, 5, 4, 'I AM SATISFIED WITH THE OVERALL QUALITY OF THE LIBRARY''S FACILITIES, RESOURCES, AND SERVICES.'),
    (25, 5, 5, 'I WOULD RECOMMEND THE LIBRARY RESOURCE CENTER TO OTHER STUDENTS AS A RELIABLE ACADEMIC LEARNING RESOURCE.');

-- ============================================================
--  EXAMPLE STATEMENTS (the application writes these)
-- ============================================================
-- SIGNUP (one transaction)
-- INSERT INTO `account_table` (`name`, `username`, `password`) VALUES ('DITCHON, JOHNCEL VIC D.', 'juan', 'juan123');
-- INSERT INTO `account_role_table` (`account_id`, `role_id`, `status_id`, `date_created`)
-- VALUES (LAST_INSERT_ID(),
--         (SELECT `role_id` FROM `role_table` WHERE `role_code` = 'STUDENT'),
--         (SELECT `status_id` FROM `account_status_table` WHERE `status_code` = 'PENDING'),
--         CURDATE());
-- INSERT INTO `program_table` (`program_name`, `institute_id`) VALUES ('BS INFORMATION TECHNOLOGY', 1)
-- ON DUPLICATE KEY UPDATE `program_id` = LAST_INSERT_ID(`program_id`);
-- INSERT INTO `student_info_table`
-- (`account_id`, `school_id`, `last_name`, `first_name`, `middle_initial`, `program_id`, `year_level`, `contact_number`, `address`)
-- VALUES (LAST_INSERT_ID(), '2026-00123', 'DITCHON', 'JOHNCEL VIC', 'D', 1, '1ST YEAR', '0917 123 4567', 'PUROK 1, TANGUB CITY');

-- SIGN IN
-- SELECT a.`account_id`, a.`name`, a.`username`, rt.`role_code` AS `role`, st.`status_code` AS `status`
-- FROM `account_table` a
-- LEFT JOIN `account_role_table` ar ON ar.`account_id` = a.`account_id`
-- LEFT JOIN `role_table` rt ON rt.`role_id` = ar.`role_id`
-- LEFT JOIN `account_status_table` st ON st.`status_id` = ar.`status_id`
-- WHERE BINARY a.`username` = BINARY 'juan' AND BINARY a.`password` = BINARY 'juan123'
-- LIMIT 1;

-- ADD BOOK
-- INSERT INTO `books_table` (`book_title`, `author`, `year_published`, `copies`, `date_added`)
-- VALUES ('NOLI ME TANGERE', 'JOSE RIZAL', 1887, 4, CURDATE());

-- BORROW BOOK (one borrower row, one loan row per copy)
-- INSERT INTO `borrowers_table`
-- (`last_name`, `first_name`, `middle_name`, `id_number`, `contact_number`, `role_id`, `program_id`, `year_level`)
-- VALUES ('DELA CRUZ', 'JUAN', 'SANTOS', '2026-00123', '09171234567',
--         (SELECT `role_id` FROM `role_table` WHERE `role_code` = 'STUDENT'), 1, '3RD YEAR');
-- INSERT INTO `borrow_transactions_table` (`borrower_id`, `book_id`, `date_borrow`, `date_return`)
-- VALUES (LAST_INSERT_ID(), 1, NOW(), NULL);

-- MARK RETURNED
-- UPDATE `borrow_transactions_table` SET `date_return` = NOW() WHERE `borrow_id` = 1;

-- STUDENT REQUEST
-- INSERT INTO `borrow_requests_table`
-- (`account_id`, `book_id`, `copies_requested`, `request_date`, `status_id`)
-- VALUES (2, 1, 1, NOW(), (SELECT `request_status_id` FROM `request_status_table` WHERE `status_code` = 'PENDING'));

-- APPROVE REQUEST
-- UPDATE `borrow_requests_table`
-- SET `status_id` = (SELECT `request_status_id` FROM `request_status_table` WHERE `status_code` = 'APPROVED'),
--     `processed_by` = 1, `processed_date` = NOW(), `borrow_id` = 10
-- WHERE `request_id` = 1;

-- DELETE BOOK (only when it has no loans)
-- DELETE FROM `books_table` WHERE `book_id` = 1;

-- DELETE ACCOUNT (role and profile rows cascade)
-- DELETE FROM `account_table` WHERE `account_id` = 2;
