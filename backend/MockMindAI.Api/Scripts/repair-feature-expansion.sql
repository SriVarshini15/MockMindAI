IF COL_LENGTH('Students', 'AvatarKey') IS NULL
BEGIN
    ALTER TABLE Students
    ADD AvatarKey nvarchar(max) NOT NULL
        CONSTRAINT DF_Students_AvatarKey DEFAULT 'mentor';
END;

IF COL_LENGTH('Students', 'IsAdmin') IS NULL
BEGIN
    ALTER TABLE Students
    ADD IsAdmin bit NOT NULL
        CONSTRAINT DF_Students_IsAdmin DEFAULT 0;
END;

IF COL_LENGTH('Students', 'IsDisabled') IS NULL
BEGIN
    ALTER TABLE Students
    ADD IsDisabled bit NOT NULL
        CONSTRAINT DF_Students_IsDisabled DEFAULT 0;
END;

IF COL_LENGTH('InterviewAttempts', 'DurationMinutes') IS NULL
BEGIN
    ALTER TABLE InterviewAttempts
    ADD DurationMinutes int NOT NULL
        CONSTRAINT DF_InterviewAttempts_DurationMinutes DEFAULT 0;
END;

IF COL_LENGTH('InterviewAttempts', 'IsTimedMode') IS NULL
BEGIN
    ALTER TABLE InterviewAttempts
    ADD IsTimedMode bit NOT NULL
        CONSTRAINT DF_InterviewAttempts_IsTimedMode DEFAULT 0;
END;

IF COL_LENGTH('InterviewAttempts', 'SkillScoresJson') IS NULL
BEGIN
    ALTER TABLE InterviewAttempts
    ADD SkillScoresJson nvarchar(max) NOT NULL
        CONSTRAINT DF_InterviewAttempts_SkillScoresJson DEFAULT '{}';
END;

IF COL_LENGTH('InterviewAttempts', 'WasAutoSubmitted') IS NULL
BEGIN
    ALTER TABLE InterviewAttempts
    ADD WasAutoSubmitted bit NOT NULL
        CONSTRAINT DF_InterviewAttempts_WasAutoSubmitted DEFAULT 0;
END;

IF EXISTS (SELECT 1 FROM Students)
BEGIN
    UPDATE Students
    SET IsAdmin = 1
    WHERE Id = (SELECT MIN(Id) FROM Students);
END;
