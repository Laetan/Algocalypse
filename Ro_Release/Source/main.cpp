#include <iostream>
#include <stdlib.h>
#include <vector>
#include <string>
#include <string.h>
#include <cfloat>
//#include <chrono>
#include <time.h>
#include <fstream>
#include <windows.h>

using namespace std;

typedef struct node{
    int id;
    float value;
    int prev;
}node;


/**
 * @brief main : Calcul de dijkstra
 * @return
 */
/*
int main()
{
    clock_t tStart = clock();
   vector<vector<float> > adjacent;

   for(int i = 0; i < 6; ++i){
       //adjacent[i] = new vector<float>();
       vector<float> t;
       for(int i = 0; i < 6; ++i)
           t.push_back(FLT_MAX);
       adjacent.push_back(t);
       adjacent[i][i] = 1;
   }
   adjacent[0][1] = 1;
   adjacent[0][3] = 3.5;
   adjacent[1][2] = 4;
   adjacent[1][4] = 3;
   adjacent[3][4] = 3;
   adjacent[4][2] = -2;
   adjacent[2][5] = 3;
   adjacent[2][3] = 1;
   adjacent[4][5] = 2;


   node nodes[6];
   for(int i = 0; i < 6; i++){
       nodes[i].id = i;
       nodes[i].value = FLT_MAX;
   }
   nodes[0].value = 0;
   nodes[0].prev = -1;

   vector<int> V;
   V.push_back(0);

    while(!V.empty()){
        node I = nodes[V.at(0)];
        int index = 0;
        for(int i = 0; i<V.size(); ++i){
            if(nodes[V.at(i)].value < I.value){
                I = nodes[V.at(i)];
                index = i;
            }
        }
        V.erase(V.begin()+index);
        for(int i = 0; i < 6; ++i){
            if(adjacent[I.id][i]==FLT_MAX || I.id == i)
                continue;
            node *J = &(nodes[i]);
            if(J->value > I.value + adjacent[I.id][J->id]){
                J->value = I.value + adjacent[I.id][J->id];
                J->prev = I.id;
                bool has = false;
                for(int j = 0; j<V.size(); ++j)
                    if(V.at(j)==J->id)
                        has = true;
                if(!has)V.push_back(J->id);
            }
        }
    }

    for(int i = 1; i <6; i++){
        cout<<"Path from 1 to "<<i+1<<" :"<<endl;
        int n = i;
        vector<int> path;
        path.clear();
        float len = nodes[n].value;
        while(n >= 0){
            path.insert(path.begin(), 1, n);
            n=nodes[n].prev;
        }
        for(int i = 0; i<path.size()-1; i++)
            cout<<path[i]+1<<" -> ";

         printf("%d\n",path.at(path.size()-1)+1);
         cout<<"Length : "<<len<<endl<<endl;
    }

    printf("Time taken: %.5fs\n", (double)(clock() - tStart)/CLOCKS_PER_SEC);

    return 0;
}*/

/**
 * @brief pathHasNode : verifie que le chemin path comporte le noeud n
 * @param path
 * @param n
 * @return vrai si le noeud y est
 */
bool pathHasNode(vector<int> path, int n){
    for(int j = 0; j<path.size(); j++){
        if(path[j]==n)
           return true;
    }
    return false;
}

bool pathHasNode(vector<vector<int> >path, int n){
    for(int j = 0; j<path.size(); j++){
        for(int i = 0; i<path[j].size(); i++){
            if(path[j][i]==n)
               return true;
        }
    }
    return false;
}

/**
 * @brief getPath : trouve un chemin non bloqué du noeud n au noeud de fin (récursif)
 * @param flow
 * @param border
 * @param path
 * @param n noeud de départ
 * @param end noeud de fin
 * @return vrai si un chemin est trouvé
 */
bool getPath(vector<vector<float> > *flow, vector<vector<float> > *border, vector<int> *path, int n, int end){
    bool pass = false;
    if(pathHasNode(*path,n)) return false;
    path->push_back(n);

    for(int i = 0; i < flow->size(); i++){
        if( n!=i && (*flow)[n][i] != FLT_MAX && (*flow)[n][i]<(*border)[n][i])
            pass = pass || getPath(flow,border, path, i, end);
    }
    if(n == end) pass = true;
    if(!pass)path->pop_back();
    return pass;
}

/**
 * @brief readParam parse les infos du fichier param.txt
 * @param flow
 * @param border
 * @return le nombre de noeud trouvé dans le fichier, ou -1 en cas d'erreur
 */
int readParam(vector<vector<float> > &flow,vector<vector<float> > &border){
    ifstream file;
    wchar_t wbuffer[MAX_PATH];
    char path[MAX_PATH];
    GetModuleFileName(NULL, wbuffer, MAX_PATH);
    int pathLength = wcstombs(path, wbuffer, MAX_PATH)-6;
    path[pathLength]='\0';
    strcat(path, "Ressources\\param.txt");
    file.open(path);

    char buffer[100];
    if(!file.is_open()){
        cout<<"Cannot open file : "<<path<<endl;
        return 0;
    }
    int mat=0;
    int nodeNum=0;
    cout<<"Parsing param :"<<endl;
    //file.getline(buffer, 100);
    while(!file.eof()){
        file.getline(buffer, 100);
        if(strlen(buffer)==0 || buffer[0]=='/'){
            if(flow.size()>0)mat++;
            //file.getline(buffer, 100);
            cout<<endl;
            continue;
        }
        cout<<buffer<<endl;
        vector<char*> char_array;
        char_array.clear();
        char* data = strtok(buffer,";");
        while(data){
            char_array.push_back(data);
            data = strtok(NULL,";");
        }

        if(nodeNum == 0) nodeNum = char_array.size();
        else if (nodeNum!=char_array.size()) {
            cout<<"Inconsistent data!"<<endl;
            file.close();
            return -1;
        }

        vector<float> float_array;
        float_array.clear();
        for(int i = 0; i < char_array.size(); i++){
            if(strcmp(char_array[i],"-")==0){
                float_array.push_back(FLT_MAX);
                 //cout<<"INF"<<endl;
            }
            else{
                float_array.push_back(atof(char_array[i]));
                //cout<<atof(char_array[i])<<endl;
            }
        }

        if(mat==0) flow.push_back(float_array);
        else border.push_back(float_array);
        //file.getline(buffer, 100);
    }
    cout<<"param loaded"<<endl;
    file.close();
    return nodeNum;

}

/**
 * @brief compute_flow Calcule les maximii des flots
 * @param flow matrice des flots
 * @param border extremum des flots
 */
void compute_flow(vector<vector<float> > &flow,vector<vector<float> > &border){
    cout<<"Calculating flow"<<endl<<endl;
    while(true){
        vector<int> path;
        path.clear();
        if(!getPath(&flow,&border,&path,0,flow.size()-1))
            break;

        float min_c = FLT_MAX;
        for(int i = 0; i < path.size()-1; i++){
            int d = border[path[i]][path[i+1]] - flow[path[i]][path[i+1]];
            if(min_c> d) min_c = d;
        }
        for(int i = 0; i < path.size()-1; i++){
            flow[path[i]][path[i+1]]+= min_c;
        }
    }
}




int main(){

    vector<vector<float> > flow, border;

    int nodeNum = readParam(flow,border);

    if(nodeNum <= 0){
        char t[2];
        cout<<"Press any key to exit"<<endl;
        cin.getline(t,1);
        return 0;
    }
    cout<<nodeNum<<" nodes detected."<<endl<<endl;
    compute_flow(flow,border);


    cout<<"Result :"<<endl;

    for(int i = 0; i < flow.size(); i++){
        for(int j = 0; j < flow.size(); j++){
            cout<<"\t";
            if(flow[i][j]<FLT_MAX)
                cout<<flow[i][j]<<" ;";
            else
                cout<<"- ;";
        }
        cout<<endl;
    }
    cout<<endl;
    char t[2];
    cout<<"Press any key to exit"<<endl;
    cin.getline(t,1);

    return 0;
}


