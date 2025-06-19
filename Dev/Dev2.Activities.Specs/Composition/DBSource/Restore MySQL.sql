CREATE DATABASE test;
USE test;

CREATE TABLE emails (
  name VARCHAR(150) DEFAULT NULL,
  email VARCHAR(45) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

INSERT INTO emails (name, email) 
VALUES ('Monk', 'dora@explorers.com');

CREATE TABLE country (
  CountryID INT DEFAULT NULL,
  Description VARCHAR(50) DEFAULT NULL,
  ID VARCHAR(45) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

INSERT INTO country (CountryID, Description, ID) 
VALUES 
  (1, 'Afghanistan', ''),
  (1, 'Afghanistan', '');

DELIMITER $$

CREATE PROCEDURE MySqlEmail()
BEGIN
    SELECT * FROM emails;
END $$

CREATE PROCEDURE Pr_CitiesGetCountries(IN name VARCHAR(50))
BEGIN
    SELECT country.CountryID,
           country.Description
    FROM test.country
    WHERE country.Description LIKE CONCAT('%', name, '%');
END $$

DELIMITER ;