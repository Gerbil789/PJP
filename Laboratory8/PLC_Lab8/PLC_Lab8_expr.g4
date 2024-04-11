grammar PLC_Lab8_expr;
 
// Lexer rules
INT : [0-9]+ ;
FLOAT : [0-9]+ '.' [0-9]* ;
BOOL : 'true' | 'false' ;
STRING : '"' (~["\\\r\n])* '"' ; 
ID : [a-zA-Z][a-zA-Z0-9]* ; 
PLUS : '+' ;
MINUS : '-' ;
STAR : '*' ;
SLASH : '/' ;
MOD : '%' ;
DOT : '.' ;
LESS : '<' ;
GREATER : '>' ;
EQUAL : '==' ;
NOTEQUAL : '!=' ;
ASSIGN : '=' ;
AND : '&&' ;
OR : '||' ;
NOT : '!' ;
SEMI : ';' ;
COMMA : ',' ;
LPAREN : '(' ;
RPAREN : ')' ;
LBRACE : '{' ;
RBRACE : '}' ;
WS : [ \t\r\n]+ -> skip ; 
COMMENT : '//' .? '\n' -> skip ; 

// Parser rules
program : statement* ;

statement
    : emptyCommand
    | declarationStatement
    | expressionStatement
    | readStatement
    | writeStatement
    | block
    | ifStatement
    | whileStatement
    | doWhileStatement
    ;

emptyCommand : SEMI ;
declarationStatement : type ID (COMMA ID)* SEMI ;
expressionStatement : expression SEMI ;
readStatement : 'read' ID (COMMA ID)* SEMI ;
writeStatement : 'write' expression (COMMA expression)* SEMI ;
block : LBRACE statement* RBRACE ;
ifStatement : 'if' LPAREN expression RPAREN statement ('else' statement)? ;
whileStatement : 'while' LPAREN expression RPAREN statement ;


doWhileStatement : 'do' statement 'while' LPAREN expression RPAREN SEMI ;


expression
    : expression (DOT | MOD | PLUS | MINUS | STAR | SLASH | LESS | GREATER | EQUAL | NOTEQUAL | AND | OR) expression #binaryExpr
    | MINUS expression #unaryMinusExpr
    | NOT expression   #notExpr
    | LPAREN expression RPAREN #parenExpr
    | ID ASSIGN expression     #assignExpr
    | INT                      #intExpr
    | FLOAT                    #floatExpr
    | BOOL                     #boolExpr
    | STRING                   #stringExpr
    | ID                       #idExpr
    ;


type : 'int' | 'float' | 'bool' | 'string' | 'error' ;