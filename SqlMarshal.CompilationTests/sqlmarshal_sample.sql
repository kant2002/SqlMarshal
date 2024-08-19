CREATE TABLE person (
person_id int not null identity primary key,
person_name nvarchar(100) null
)
GO

CREATE OR ALTER PROCEDURE persons_list
AS
SELECT * from person
GO

CREATE OR ALTER PROCEDURE persons_by_id
	@person_id int
AS
SELECT * from person
WHERE person_id = @person_id
GO

CREATE OR ALTER PROCEDURE persons_by_page
	@page_no int,
	@total_count int OUTPUT
AS
SELECT * from person
ORDER BY person_name
    OFFSET (@page_no * 10) ROWS  
    FETCH NEXT 10 ROWS ONLY;  

select @total_count = count(*) from person
GO

-- dummy data

declare @i int
set @i = 100
while @i > 0
begin
	insert into person(person_name)
	values(concat('Person ', convert(varchar, @i)))

	set @i = @i - 1
end
GO

CREATE TABLE [user] (
user_id int not null identity primary key,
user_name nvarchar(100) null
)
GO



CREATE OR ALTER PROCEDURE users_list
AS
SELECT * from [user]
GO