var app = require('express')();
var server = require('http').Server(app);
var io = require('socket.io')(server);

server.listen(3000);

// Variables globale para el servidor
var spawnPoints = [];
var clients = [];




app.get("/", function(req,res){
    res.send('Hey, ve para atrás"/"');
});

io.on('connection', function(socket){
    
    var currentPlayer = {};
    currentPlayer.nombre = "anonimo";
    
    socket.on('conectar jugador', function(){
        console.log(currentPlayer.nombre + "recv: Jugador conectado");
        for(var i = 0; i < clients.length; i++){
            var playerConnected = {
                name:clients[i].nombre,
                position:clients[i].position,
                rotation:clients[i].rotation,
                salud:clients[i].salud,
                personaje:clients[i].personaje,
                muertes:clients[i].muertes
            };
            // en el actual juego necesitamos decirte sobre 
            socket.emit('otro jugador conectado',playerConnected);
            console.log(currentPlayer.nombre + " emit: otro jugador conectado: " + JSON.stringify(playerConnected));
        }
        //socket.broadcast.emit('play',currentPlayer);
        
    });
    socket.on('play', function(data){
        console.log(currentPlayer.nombre + " rcv: play: " + JSON.stringify(data));
        if (clients.length == 0){
            spawnPoints = [];
            data.spawnPoints.forEach(function(_spawnPoint){
                var spawnPoint = {
                    position: _spawnPoint.position,
                    rotation: _spawnPoint.rotation
                };
                spawnPoints.push(spawnPoint);
                
            });
        }
        
        var randomSpawnPoint = spawnPoints[Math.floor(Math.random()*spawnPoints.length)];
        currentPlayer = {
            nombre:data.nombre,
            position:randomSpawnPoint.position,
            rotation:randomSpawnPoint.rotation,
            salud:100
        };
        console.log("---> La posicion seleccionada es: " + randomSpawnPoint);
        clients.push(currentPlayer);
        //decirte que entraste
        socket.emit('play',currentPlayer);
        console.log(currentPlayer.nombre + " emit: play2: "+JSON.stringify(currentPlayer));
        socket.broadcast.emit('Entro un jugador nuevo',currentPlayer);
        
    });
    
    socket.on('enemies', function(data){
        
        var randomSpawnPoint = spawnPoints[Math.floor(Math.random()*spawnPoints.length)];
        currentPlayer = {
            nombre:data.nombre,
            position:randomSpawnPoint.position,
            rotation:randomSpawnPoint.rotation,
            salud:100
        };
        clients.push(currentPlayer);
        //decirte que entraste
        console.log(currentPlayer.nombre + " emit: play: "+JSON.stringify(currentPlayer));
        socket.broadcast.emit('play',currentPlayer);
        
    });
    socket.on('player move',function(data){
        //console.log("recv: move: " + JSON.stringify(data));
        currentPlayer.position = data.position;
        socket.broadcast.emit('player move',currentPlayer);
    });
    
    socket.on('player turn',function(data){
        //console.log("recv: turn: " + JSON.stringify(data));
        currentPlayer.rotation = data.rotation;
        socket.broadcast.emit('player turn',currentPlayer);
    });
    
    
    socket.on('player shoot',function(){
        console.log(currentPlayer.nombre + "recv: shoot");
        var data = {
            nombre: currentPlayer.nombre
        };
        console.log(currentPlayer.nombre + "bcst: shoot" + JSON.stringify(data));
        socket.emit('player shoot',data);
        socket.broadcast.emit('player shoot',data);
    });
    
    
    socket.on('player dance',function(){
        console.log(currentPlayer.nombre + "recv: dance");
        var data = {
            nombre: currentPlayer.nombre
        };
        console.log(currentPlayer.nombre + "bcst: dance" + JSON.stringify(data));
        socket.emit('player dance',data);
        socket.broadcast.emit('player dance',data);
    });
    
    
    
    socket.on('salud',function(data){
        console.log(currentPlayer.nombre + "recv: salud: " + JSON.stringify(data));
        
        if(data.from == currentPlayer.nombre){
            var indexDamaged = 0;
            ///Aca estoy haciendo algo que no debería, pero me vale.
            clients = clients.map(function(client,index){
                if(client.nombre == data.nombre){
                    indexDamaged = index;
                    client.salud -= data.saludCambio;
                }
                return client;
            });
            /// hasta acá
            
            var response = {
                nombre: clients[indexDamaged].nombre,
                salud: clients[indexDamaged].salud
            };
            
            console.log (currentPlayer.nombre + "bcst: salud: " + JSON.stringify(response));
            socket.emit('salud', response);
            socket.broadcast.emit('salud',response);
        }
        currentPlayer.rotation = data.rotation;
        socket.broadcast.emit('player turn',currentPlayer);
    });
    
    socket.on('muerte',function(data){
        console.log(currentPlayer.nombre + " recv: muerte: " + JSON.stringify(data));
        console.log(currentPlayer.nombre + " -->: " + data.nombre + " / " + currentPlayer.nombre);
        if(data.nombre == currentPlayer.nombre){
            var indexDamaged = 0;
            ///Aca estoy haciendo algo que no debería, pero me vale.
            clients = clients.map(function(client,index){
                if(client.nombre == data.nombre){
                    indexDamaged = index;
                    client.muertes = data.muertesCambio;
                }
                return client;
            });
            /// hasta acá
            
            var response = {
                nombre: clients[indexDamaged].nombre,
                muertes: clients[indexDamaged].muertes
            };
            
            console.log (currentPlayer.nombre + " bcst: muerte: " + JSON.stringify(response));
            socket.emit('muerte', response);
            socket.broadcast.emit('muerte',response);
        }
        currentPlayer.rotation = data.rotation;
        socket.broadcast.emit('player turn',currentPlayer);
    });
    
    socket.on('player disconnected', function(){
        console.log(currentPlayer.nombre + " desconectado!!!");
        socket.broadcast.emit('otro jugador desconectado', currentPlayer);
        console.log(curren.nombre + "bcst: otro jugador desconectado" + JSON.stringify(currentPlayer));
        for(var i =0; i<clients.length; i++){
            if(clients[i].nombre == currentPlayer.nombre){
                clients.splice(i,1);
            }
        }
    });
});
console.log('--- El servidor se está ejecutando. ');
