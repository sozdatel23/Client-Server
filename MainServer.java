package com.company;

import java.io.*;
import java.net.*;

public class Main{
    public static final int PORT = 8080;
    public static ServerSocket server;
    private static BufferedReader in;
    private static BufferedWriter out;

    public static void main(String[] args) throws IOException {
        new Main().bind(2000,2046);
        System.out.println("Started: " + server);
        try {
            // Блокирует до тех пор, пока не возникнет соединение:
            Socket socket = server.accept();
            try {
                System.out.println("Connection accepted: " + socket);
                in = new BufferedReader(new InputStreamReader(socket.getInputStream()));
                out = new BufferedWriter(new OutputStreamWriter(socket.getOutputStream()));

                Boolean a = true;
                while (a == true) {
                    String word = in.readLine();
                    System.out.println("Client: " + word);

                    if(word.equals("exit")){
                        out.write("Ok, тогда " + word + "\n");
                        a = false;
                    }
                    if(word.equals("Do you understand me?")){
                        out.write("Yes, i do" + "\n");
                    }

                    out.write("Вы написали  " + word + "\n");
                    out.flush();
                }
            }
            finally {
                System.out.println("closing...");
                socket.close();
            }
        }
        finally {
            server.close();
        }
    }

    public Main bind(int fromPort, int toPort) {
        System.out.println("Creating server");

        for (int i = fromPort; i <= toPort; i++) {
            try {
                System.out.println("Trying to create server on port " + i);
                server = new ServerSocket(i);
                break;
            } catch (IOException e) {
                System.out.println("Failed to create server on port " + i);
            }
        }

        if (server != null) {
            System.out.println("Server successfully created on port " + server.getLocalPort());
        } else {
            System.out.println("Failed to create server on ports " + fromPort + "-" + toPort);
        }

        return this;
    }
}
