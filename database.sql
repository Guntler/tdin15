drop table if exists User;
Drop table if exists DOrder;
drop table if exists DTransaction;
drop table if exists transStatusEnum;
drop table if exists transTypeEnum;
drop table if exists Diginote;

CREATE TABLE transTypeEnum (id INTEGER PRIMARY KEY AUTOINCREMENT, transType TEXT);
INSERT INTO transTypeEnum(transType) VALUES('BUY'), ('SELL');

CREATE TABLE transStatusEnum (id INTEGER PRIMARY KEY AUTOINCREMENT, transStatus TEXT);
INSERT INTO transStatusEnum(transStatus) VALUES('ACTIVE'), ('FULFILLED'), ('CANCELLED');


CREATE TABLE User (
	id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
	name VARCHAR(256),
	nickname VARCHAR(256) UNIQUE,
	password VARCHAR(256)
	
);

CREATE TABLE DOrder (
	id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
	type REFERENCES transTypeEnum(id) NOT NULL,
	status REFERENCES transStatusEnum(id) NOT NULL,
	date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	source INTEGER REFERENCES User(id),
	value DOUBLE NOT NULL DEFAULT 0,
	amount INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE DTransaction (
	id INTEGER REFERENCES DOrder(id),
	date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	destination INTEGER REFERENCES User(id),
	value DOUBLE NOT NULL DEFAULT 0
);

CREATE TABLE Diginote (
	id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL ,
	value int DEFAULT 1,
	owner INTEGER REFERENCES User(id)
);