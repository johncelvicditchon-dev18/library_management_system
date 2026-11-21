-- ============================================================
--  libsys_database -> libsys_database_3nf
--  Builds a 3NF staging copy and leaves the source untouched.
--  Supports both the legacy schema and an already normalized schema.
--
--  mysql -u root -P 3306 < database/normalize_to_3nf.sql
-- ============================================================

DROP DATABASE IF EXISTS `libsys_database_3nf`;
CREATE DATABASE `libsys_database_3nf`
    DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `libsys_database_3nf`;

CREATE TABLE `role_table` (
    `role_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `role_code` VARCHAR(20) NOT NULL,
    UNIQUE KEY `ux_role_code` (`role_code`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

INSERT INTO `role_table` (`role_code`) VALUES
    ('STUDENT'), ('FACULTY'), ('STAFF'), ('LIBRARIAN');

CREATE TABLE `account_status_table` (
    `status_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `status_code` VARCHAR(20) NOT NULL,
    UNIQUE KEY `ux_account_status_code` (`status_code`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

INSERT INTO `account_status_table` (`status_code`) VALUES
    ('PENDING'), ('ACTIVE'), ('INACTIVE'), ('REJECTED');

CREATE TABLE `request_status_table` (
    `request_status_id` INT AUTO_INCREMENT PRIMARY KEY,
    `status_code`       VARCHAR(20) NOT NULL,
    UNIQUE KEY `ux_request_status_code` (`status_code`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

INSERT INTO `request_status_table` (`status_code`) VALUES
    ('PENDING'), ('APPROVED'), ('REJECTED'), ('CANCELLED'), ('CLAIMED');

CREATE TABLE `institute_table` (
    `institute_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `institute_code` VARCHAR(10)  NOT NULL,
    `institute_name` VARCHAR(120) NOT NULL,
    UNIQUE KEY `ux_institute_code` (`institute_code`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE `program_table` (
    `program_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `program_name` VARCHAR(120) NOT NULL,
    `institute_id` INT          NOT NULL,
    UNIQUE KEY `ux_program_name_institute` (`program_name`, `institute_id`),
    CONSTRAINT `fk_program_institute`
        FOREIGN KEY (`institute_id`) REFERENCES `institute_table` (`institute_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE `account_table` (
    `account_id`      INT AUTO_INCREMENT PRIMARY KEY,
    `name`            VARCHAR(250) NOT NULL,
    `username`        VARCHAR(250) NOT NULL,
    `password`        VARCHAR(255) NOT NULL,
    `profile_picture` MEDIUMBLOB   NULL,
    UNIQUE KEY `ux_account_username` (`username`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE `account_role_table` (
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

CREATE TABLE `student_info_table` (
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

CREATE TABLE `faculty_info_table` (
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

CREATE TABLE `staff_info_table` (
    `staff_info_id`  INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`     INT          NOT NULL,
    `office`         VARCHAR(120) NOT NULL,
    `contact_number` VARCHAR(20)  NOT NULL DEFAULT '',
    UNIQUE KEY `ux_staff_account` (`account_id`),
    CONSTRAINT `fk_staff_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE `librarian_info_table` (
    `librarian_info_id` INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`        INT NOT NULL,
    `contact_number`    VARCHAR(20) NOT NULL DEFAULT '',
    UNIQUE KEY `ux_librarian_account` (`account_id`),
    CONSTRAINT `fk_librarian_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE `books_table` (
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

CREATE TABLE `borrowers_table` (
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

CREATE TABLE `borrow_transactions_table` (
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

CREATE TABLE `borrow_requests_table` (
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

CREATE TABLE `attendance_table` (
    `attendance_id` INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`    INT      NOT NULL,
    `scan_time`     DATETIME NOT NULL,
    KEY `idx_attendance_account` (`account_id`),
    KEY `idx_attendance_time` (`scan_time`),
    CONSTRAINT `fk_attendance_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE `evaluation_period_table` (
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

CREATE TABLE `evaluation_question_table` (
    `question_id`   INT AUTO_INCREMENT PRIMARY KEY,
    `section_no`    INT          NOT NULL,
    `question_no`   INT          NOT NULL,
    `question_text` VARCHAR(255) NOT NULL,
    UNIQUE KEY `ux_evaluation_question` (`section_no`, `question_no`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE `evaluation_table` (
    `evaluation_id` INT AUTO_INCREMENT PRIMARY KEY,
    `account_id`    INT      NULL,
    `period_id`     INT      NOT NULL,
    `submitted_at`  DATETIME NOT NULL,
    UNIQUE KEY `uq_evaluation_period_account` (`period_id`, `account_id`),
    KEY `idx_evaluation_account` (`account_id`),
    KEY `idx_evaluation_period` (`period_id`),
    CONSTRAINT `fk_evaluation_account`
        FOREIGN KEY (`account_id`) REFERENCES `account_table` (`account_id`)
        ON DELETE SET NULL,
    CONSTRAINT `fk_evaluation_period`
        FOREIGN KEY (`period_id`) REFERENCES `evaluation_period_table` (`period_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE `evaluation_respondent_table` (
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

CREATE TABLE `evaluation_answer_table` (
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

INSERT INTO `evaluation_question_table` (`question_id`, `section_no`, `question_no`, `question_text`) VALUES
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

DELIMITER $$

CREATE PROCEDURE `copy_libsys_to_3nf`()
BEGIN
    DECLARE legacy_schema INT DEFAULT 0;
    DECLARE legacy_evaluation INT DEFAULT 0;
    DECLARE refined_evaluation INT DEFAULT 0;

    SET legacy_schema = (
        SELECT COUNT(*) FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = 'libsys_database'
          AND TABLE_NAME = 'account_role_table'
          AND COLUMN_NAME = 'role'
    );

    SET legacy_evaluation = (
        SELECT COUNT(*) FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = 'libsys_database'
          AND TABLE_NAME = 'evaluation_table'
          AND COLUMN_NAME = 'role_id'
    );

    SET refined_evaluation = (
        SELECT COUNT(*) FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = 'libsys_database'
          AND TABLE_NAME = 'evaluation_respondent_table'
          AND COLUMN_NAME = 'role_id'
    );

    SET FOREIGN_KEY_CHECKS = 0;

    INSERT INTO `institute_table` (`institute_id`, `institute_code`, `institute_name`)
    SELECT `institute_id`, `institute_code`, `institute_name`
    FROM `libsys_database`.`institute_table`;

    INSERT INTO `program_table` (`program_id`, `program_name`, `institute_id`)
    SELECT `program_id`, `program_name`, `institute_id`
    FROM `libsys_database`.`program_table`;

    INSERT INTO `account_table` (`account_id`, `name`, `username`, `password`, `profile_picture`)
    SELECT `account_id`, `name`, `username`, `password`, `profile_picture`
    FROM `libsys_database`.`account_table`;

    IF legacy_schema > 0 THEN
        INSERT IGNORE INTO `account_role_table`
            (`account_id`, `role_id`, `status_id`, `date_created`)
        SELECT ar.`account_id`,
               COALESCE((SELECT rt.`role_id` FROM `role_table` rt
                          WHERE rt.`role_code` = CASE UPPER(TRIM(ar.`role`))
                                                     WHEN 'ADMIN' THEN 'LIBRARIAN'
                                                     WHEN 'OFFICER' THEN 'STAFF'
                                                     ELSE UPPER(TRIM(ar.`role`))
                                                 END COLLATE utf8mb4_general_ci),
                        (SELECT rt.`role_id` FROM `role_table` rt WHERE rt.`role_code` = 'STUDENT')),
               COALESCE((SELECT st.`status_id` FROM `account_status_table` st
                          WHERE st.`status_code` = UPPER(TRIM(ar.`status`)) COLLATE utf8mb4_general_ci),
                        (SELECT st.`status_id` FROM `account_status_table` st WHERE st.`status_code` = 'PENDING')),
               COALESCE(STR_TO_DATE(ar.`date_created`, '%M %e, %Y'),
                        STR_TO_DATE(ar.`date_created`, '%Y-%m-%d'),
                        CURDATE())
        FROM `libsys_database`.`account_role_table` ar
        ORDER BY ar.`role_id`;
    ELSE
        INSERT INTO `account_role_table`
            (`account_id`, `role_id`, `status_id`, `date_created`)
        SELECT `account_id`, `role_id`, `status_id`, `date_created`
        FROM `libsys_database`.`account_role_table`;
    END IF;

    INSERT INTO `student_info_table`
        (`student_info_id`, `account_id`, `school_id`, `last_name`, `first_name`,
         `middle_initial`, `program_id`, `year_level`, `contact_number`, `address`)
    SELECT `student_info_id`, `account_id`, `school_id`, `last_name`, `first_name`,
           `middle_initial`, `program_id`, `year_level`, `contact_number`, `address`
    FROM `libsys_database`.`student_info_table`;

    INSERT INTO `faculty_info_table`
        (`faculty_info_id`, `account_id`, `institute_id`, `contact_number`)
    SELECT `faculty_info_id`, `account_id`, `institute_id`, `contact_number`
    FROM `libsys_database`.`faculty_info_table`;

    INSERT INTO `staff_info_table`
        (`staff_info_id`, `account_id`, `office`, `contact_number`)
    SELECT `staff_info_id`, `account_id`, `office`, `contact_number`
    FROM `libsys_database`.`staff_info_table`;

    INSERT INTO `librarian_info_table`
        (`librarian_info_id`, `account_id`, `contact_number`)
    SELECT `librarian_info_id`, `account_id`, `contact_number`
    FROM `libsys_database`.`librarian_info_table`;

    IF legacy_schema > 0 THEN
        INSERT INTO `books_table`
            (`book_id`, `book_title`, `author`, `year_published`, `copies`, `date_added`, `book_image`)
        SELECT `book_id`, `book_title`, `author`, `year_published`,
               GREATEST(1, COALESCE(CAST(`copy` AS UNSIGNED), 1)),
               COALESCE(STR_TO_DATE(`date_added`, '%M %e, %Y'),
                        STR_TO_DATE(`date_added`, '%Y-%m-%d'),
                        CURDATE()),
               `book_image`
        FROM `libsys_database`.`books_table`;

        INSERT IGNORE INTO `program_table` (`program_name`, `institute_id`)
        SELECT DISTINCT UPPER(TRIM(b.`program`)), i.`institute_id`
        FROM `libsys_database`.`borrowers_table` b
        JOIN `institute_table` i
          ON i.`institute_code` = UPPER(TRIM(b.`institute`)) COLLATE utf8mb4_general_ci
        WHERE b.`program` IS NOT NULL AND TRIM(b.`program`) <> '';

        DROP TEMPORARY TABLE IF EXISTS `tmp_borrower_migrate`;
        CREATE TEMPORARY TABLE `tmp_borrower_migrate` AS
        SELECT b.`borrower_id`,
               (SELECT a.`account_id`
                  FROM `account_table` a
                  JOIN `student_info_table` s ON s.`account_id` = a.`account_id`
                 WHERE s.`school_id` = b.`id_number`
                 LIMIT 1) AS `account_id`,
               COALESCE(NULLIF(UPPER(TRIM(b.`last_name`)), ''),
                        CASE WHEN LOCATE(',', b.`borrower_name`) > 0
                             THEN UPPER(TRIM(SUBSTRING_INDEX(b.`borrower_name`, ',', 1)))
                             ELSE UPPER(TRIM(SUBSTRING_INDEX(b.`borrower_name`, ' ', 1)))
                        END) AS `last_name`,
               COALESCE(NULLIF(UPPER(TRIM(b.`first_name`)), ''),
                        UPPER(TRIM(SUBSTRING_INDEX(
                            CASE WHEN LOCATE(',', b.`borrower_name`) > 0
                                 THEN TRIM(SUBSTRING(b.`borrower_name`, LOCATE(',', b.`borrower_name`) + 1))
                                 ELSE TRIM(SUBSTRING(b.`borrower_name`, LOCATE(' ', b.`borrower_name`) + 1))
                            END, ' ', 1)))) AS `first_name`,
               COALESCE(NULLIF(UPPER(TRIM(b.`middle_name`)), ''),
                        NULLIF(UPPER(TRIM(SUBSTRING(
                            CASE WHEN LOCATE(',', b.`borrower_name`) > 0
                                 THEN TRIM(SUBSTRING(b.`borrower_name`, LOCATE(',', b.`borrower_name`) + 1))
                                 ELSE TRIM(SUBSTRING(b.`borrower_name`, LOCATE(' ', b.`borrower_name`) + 1))
                            END,
                            LOCATE(' ',
                                CASE WHEN LOCATE(',', b.`borrower_name`) > 0
                                     THEN TRIM(SUBSTRING(b.`borrower_name`, LOCATE(',', b.`borrower_name`) + 1))
                                     ELSE TRIM(SUBSTRING(b.`borrower_name`, LOCATE(' ', b.`borrower_name`) + 1))
                                END) + 1))), '')) AS `middle_name`,
               NULLIF(UPPER(TRIM(b.`id_number`)), '') AS `id_number`,
               COALESCE(UPPER(TRIM(b.`contact_number`)), '') AS `contact_number`,
               CASE
                   WHEN UPPER(TRIM(COALESCE(b.`role`, ''))) = 'FACULTY' THEN 'FACULTY'
                   WHEN UPPER(TRIM(COALESCE(b.`role`, ''))) IN ('STAFF', 'LIBRARIAN', 'OFFICER', 'ADMIN') THEN 'STAFF'
                   ELSE 'STUDENT'
               END AS `role_code`,
               NULLIF(UPPER(TRIM(b.`program`)), '') AS `program_name`,
               NULLIF(UPPER(TRIM(b.`year_level`)), '') AS `year_level`,
               NULLIF(UPPER(TRIM(b.`institute`)), '') AS `institute_code`,
               NULLIF(UPPER(TRIM(b.`office`)), '') AS `office`
        FROM `libsys_database`.`borrowers_table` b;

        ALTER TABLE `tmp_borrower_migrate`
            ADD COLUMN `program_id` INT NULL,
            ADD COLUMN `institute_id` INT NULL;

        UPDATE `tmp_borrower_migrate` t
        LEFT JOIN `program_table` p
               ON UPPER(p.`program_name`) = t.`program_name` COLLATE utf8mb4_general_ci
        LEFT JOIN `institute_table` pi ON pi.`institute_id` = p.`institute_id`
        SET t.`program_id` = p.`program_id`
        WHERE t.`program_name` IS NOT NULL
          AND (t.`institute_code` IS NULL OR t.`institute_code` = '' OR pi.`institute_code` = t.`institute_code` COLLATE utf8mb4_general_ci);

        UPDATE `tmp_borrower_migrate` t
        LEFT JOIN `institute_table` i ON i.`institute_code` = t.`institute_code` COLLATE utf8mb4_general_ci
        SET t.`institute_id` = CASE WHEN t.`program_id` IS NULL THEN i.`institute_id` ELSE NULL END;

        INSERT INTO `borrowers_table`
            (`borrower_id`, `account_id`, `last_name`, `first_name`, `middle_name`, `id_number`,
             `contact_number`, `role_id`, `program_id`, `year_level`, `institute_id`, `office`)
        SELECT t.`borrower_id`, t.`account_id`, t.`last_name`, COALESCE(t.`first_name`, ''),
               t.`middle_name`, t.`id_number`, COALESCE(t.`contact_number`, ''),
               (SELECT rt.`role_id` FROM `role_table` rt WHERE rt.`role_code` = t.`role_code` COLLATE utf8mb4_general_ci),
               t.`program_id`, t.`year_level`, t.`institute_id`, t.`office`
        FROM `tmp_borrower_migrate` t
        ORDER BY t.`borrower_id`;

        DROP TEMPORARY TABLE IF EXISTS `tmp_borrower_migrate`;
    ELSE
        INSERT INTO `books_table`
            (`book_id`, `book_title`, `author`, `year_published`, `copies`, `date_added`, `book_image`)
        SELECT `book_id`, `book_title`, `author`, `year_published`,
               GREATEST(1, `copies`), `date_added`, `book_image`
        FROM `libsys_database`.`books_table`;

        INSERT INTO `borrowers_table`
            (`borrower_id`, `account_id`, `last_name`, `first_name`, `middle_name`, `id_number`,
             `contact_number`, `role_id`, `program_id`, `year_level`, `institute_id`, `office`)
        SELECT `borrower_id`, `account_id`, `last_name`, `first_name`, `middle_name`, `id_number`,
               COALESCE(`contact_number`, ''), `role_id`, `program_id`, `year_level`,
               CASE WHEN `program_id` IS NOT NULL THEN NULL ELSE `institute_id` END, `office`
        FROM `libsys_database`.`borrowers_table`;
    END IF;

    INSERT INTO `borrow_transactions_table`
        (`borrow_id`, `borrower_id`, `book_id`, `date_borrow`, `date_return`)
    SELECT `borrow_id`, `borrower_id`, `book_id`, `date_borrow`, `date_return`
    FROM `libsys_database`.`borrow_transactions_table`;

    IF legacy_schema > 0 THEN
        INSERT INTO `borrow_requests_table`
            (`request_id`, `account_id`, `book_id`, `copies_requested`, `request_date`, `status_id`,
             `processed_by`, `processed_date`, `claimed_by`, `claim_date`, `borrow_id`)
        SELECT r.`request_id`, r.`account_id`, r.`book_id`, COALESCE(r.`copies_requested`, 1),
               r.`request_date`,
               COALESCE((SELECT rs.`request_status_id` FROM `request_status_table` rs
                          WHERE rs.`status_code` = UPPER(TRIM(r.`status`)) COLLATE utf8mb4_general_ci),
                        (SELECT rs.`request_status_id` FROM `request_status_table` rs
                          WHERE rs.`status_code` = 'PENDING')),
               r.`processed_by`, r.`processed_date`, r.`claimed_by`, r.`claim_date`, r.`borrow_id`
        FROM `libsys_database`.`borrow_requests_table` r;
    ELSE
        INSERT INTO `borrow_requests_table`
            (`request_id`, `account_id`, `book_id`, `copies_requested`, `request_date`, `status_id`,
             `processed_by`, `processed_date`, `claimed_by`, `claim_date`, `borrow_id`)
        SELECT `request_id`, `account_id`, `book_id`, `copies_requested`, `request_date`, `status_id`,
               `processed_by`, `processed_date`, `claimed_by`, `claim_date`, `borrow_id`
        FROM `libsys_database`.`borrow_requests_table`;
    END IF;

    INSERT INTO `attendance_table` (`attendance_id`, `account_id`, `scan_time`)
    SELECT `attendance_id`, `account_id`, `scan_time`
    FROM `libsys_database`.`attendance_table`;

    INSERT INTO `evaluation_period_table`
        (`period_id`, `semester`, `school_year`, `opened_at`, `closed_at`)
    SELECT p.`period_id`, p.`semester`, p.`school_year`,
           p.`opened_at`,
           CASE
               WHEN p.`opened_at` IS NOT NULL AND p.`closed_at` IS NULL
                    AND p.`period_id` <> (
                        SELECT keep_id FROM (
                            SELECT `period_id` AS keep_id
                            FROM `libsys_database`.`evaluation_period_table`
                            WHERE `opened_at` IS NOT NULL AND `closed_at` IS NULL
                            ORDER BY `opened_at` DESC, `period_id` DESC
                            LIMIT 1
                        ) AS latest_open_period
                    )
               THEN NOW()
               ELSE p.`closed_at`
           END
    FROM `libsys_database`.`evaluation_period_table` p;

    INSERT INTO `evaluation_table`
        (`evaluation_id`, `account_id`, `period_id`, `submitted_at`)
    SELECT e.`evaluation_id`, e.`account_id`, e.`period_id`, e.`submitted_at`
    FROM `libsys_database`.`evaluation_table` e;

    IF legacy_schema > 0 THEN
        INSERT INTO `evaluation_respondent_table`
            (`evaluation_id`, `role_id`, `respondent_name`)
        SELECT e.`evaluation_id`,
               COALESCE((SELECT rt.`role_id` FROM `role_table` rt
                          WHERE rt.`role_code` = CASE UPPER(TRIM(e.`respondent_role`))
                                                     WHEN 'ADMIN' THEN 'LIBRARIAN'
                                                     WHEN 'OFFICER' THEN 'STAFF'
                                                     ELSE UPPER(TRIM(e.`respondent_role`))
                                                 END COLLATE utf8mb4_general_ci),
                        (SELECT rt.`role_id` FROM `role_table` rt WHERE rt.`role_code` = 'STUDENT')),
               COALESCE(e.`respondent_name`, '')
        FROM `libsys_database`.`evaluation_table` e;
    ELSEIF legacy_evaluation > 0 THEN
        INSERT INTO `evaluation_respondent_table`
            (`evaluation_id`, `role_id`, `respondent_name`)
        SELECT `evaluation_id`, `role_id`, `respondent_name`
        FROM `libsys_database`.`evaluation_table`;
    ELSEIF refined_evaluation > 0 THEN
        INSERT INTO `evaluation_respondent_table`
            (`evaluation_id`, `role_id`, `respondent_name`)
        SELECT `evaluation_id`, `role_id`, `respondent_name`
        FROM `libsys_database`.`evaluation_respondent_table`;
    END IF;

    INSERT IGNORE INTO `evaluation_respondent_table`
        (`evaluation_id`, `role_id`, `respondent_name`)
    SELECT e.`evaluation_id`,
           (SELECT rt.`role_id` FROM `role_table` rt WHERE rt.`role_code` = 'STUDENT'),
           ''
    FROM `evaluation_table` e
    LEFT JOIN `evaluation_respondent_table` er ON er.`evaluation_id` = e.`evaluation_id`
    WHERE er.`evaluation_id` IS NULL;

    IF legacy_schema > 0 THEN
        INSERT IGNORE INTO `evaluation_answer_table`
            (`answer_id`, `evaluation_id`, `question_id`, `rating`)
        SELECT a.`answer_id`, a.`evaluation_id`,
               ((a.`section_no` - 1) * 5) + a.`question_no`, a.`rating`
        FROM `libsys_database`.`evaluation_answer_table` a
        WHERE a.`section_no` BETWEEN 1 AND 5
          AND a.`question_no` BETWEEN 1 AND 5
          AND a.`rating` BETWEEN 1 AND 5;
    ELSE
        INSERT INTO `evaluation_answer_table`
            (`answer_id`, `evaluation_id`, `question_id`, `rating`)
        SELECT `answer_id`, `evaluation_id`, `question_id`, `rating`
        FROM `libsys_database`.`evaluation_answer_table`;
    END IF;

    SET FOREIGN_KEY_CHECKS = 1;
END$$

DELIMITER ;

CALL `copy_libsys_to_3nf`();
DROP PROCEDURE `copy_libsys_to_3nf`;

SELECT 'MIGRATION COMPLETE' AS `result`,
       (SELECT COUNT(*) FROM `account_table`) AS `accounts`,
       (SELECT COUNT(*) FROM `books_table`) AS `books`,
       (SELECT COUNT(*) FROM `borrowers_table`) AS `borrowers`,
       (SELECT COUNT(*) FROM `borrow_transactions_table`) AS `loans`,
       (SELECT COUNT(*) FROM `attendance_table`) AS `attendance`,
       (SELECT COUNT(*) FROM `evaluation_table`) AS `evaluations`,
       (SELECT COUNT(*) FROM `evaluation_respondent_table`) AS `respondents`,
       (SELECT COUNT(*) FROM `evaluation_answer_table`) AS `answers`;
