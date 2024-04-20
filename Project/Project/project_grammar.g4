grammar project_grammar;

INT_KEYWORD : 'int';
FLOAT_KEYWORD : 'float';
STRING_KEYWORD : 'string';
BOOL_KEYWORD : 'bool';
SEMI: ';';
COMMA: ',';
CON : '.' ;
MUL : '*' ; 
DIV : '/' ;
MOD : '%' ;
ADD : '+' ;
SUB : '-' ;
LES : '<' ;
GRE : '>' ;
NEG : '!' ;
EQ  : '==' ;
NEQ : '!=' ;
AND : '&&' ;
OR : '||' ;
READ : 'read' ;
WRITE : 'write' ;
IF : 'if' ;
ELSE : 'else' ;
DO : 'do' ;
WHILE : 'while' ;
FOR : 'for' ;
IDENTIFIER : [a-zA-Z] ([a-zA-Z0-9]*)? ; 

FLOAT : [0-9]+'.'[0-9]+ ;
INT : [0-9]+ ; 
BOOL : ('true'|'false') ;
STRING : '"' (~["\\\r\n] | EscapeSequence)* '"';

fragment EscapeSequence
    : '\\' [btnfr"'\\]
    | '\\' ([0-3]? [0-7])? [0-7]
    ;

WS : [ \t\r\n]+ -> skip ;
COMMENT: '/*' .*? '*/' -> skip ;
LINE_COMMENT: '//' ~[\r\n]* -> skip ;

//start here!!!
program: statement+ EOF ;

statement
    : '{' statement* '}'                                        # blockOfStatements
    | primitiveType IDENTIFIER ( COMMA IDENTIFIER)* SEMI        # declaration
    | IF '(' expr ')' pos=statement (ELSE neg=statement)?       # ifElse
    | WHILE '(' expr ')' statement                              # while
    | DO statement WHILE '(' expr ')' SEMI                      # doWhile
    | FOR '(' (expr)? SEMI (expr)? SEMI (expr)? ')' statement   # for
    | READ IDENTIFIER ( COMMA IDENTIFIER)* SEMI                 # readStatement
    | WRITE expr ( COMMA expr)* SEMI                            # writeStatement
    | expr SEMI                                                 # printExpr
    | SEMI                                                      # emptyStatement
    ;

    

expr: IDENTIFIER                            # id
    | ('true'|'false')                      # bool
    | '(' expr ')'                          # parens
    | INT                                   # int
    | FLOAT                                 # float
    | STRING                                # string
    | prefix=SUB expr                       # unaryMinus
    | prefix=NEG expr                       # negation
    | expr op=(MUL|DIV|MOD) expr            # mulDivMod
    | expr op=(ADD|SUB|CON) expr            # addSubCon
    | expr op=(LES|GRE) expr                # relation
    | expr op=(EQ|NEQ) expr                 # comparison
    | expr AND expr                         # logicalAnd
    | expr OR expr                          # logicalOr
    | <assoc=right> IDENTIFIER '=' expr     # assignment
    ;

primitiveType
    : type=INT_KEYWORD
    | type=FLOAT_KEYWORD
    | type=STRING_KEYWORD
    | type=BOOL_KEYWORD
    ;