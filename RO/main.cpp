#include <iostream>
#include <vector>
#include <string>
#include <string.h>
#include <cfloat>
#include <chrono>
#include <time.h>
using namespace std;

typedef struct node{
    int id;
    float value;
    int prev;
}node;
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

bool getPath(vector<vector<float> > *flow, vector<vector<float> > *border, vector<int> *path, int n, int end){
    bool pass = false;
    if(pathHasNode(*path,n)) return false;
    path->push_back(n);
    //cout<<flow[1][1];
    for(int i = 0; i < 5; i++){
        if( n!=i && (*flow)[n][i] != FLT_MAX && (*flow)[n][i]<(*border)[n][i])
            pass = pass || getPath(flow,border, path, i, end);
    }
    if(n == end) pass = true;
    if(!pass)path->pop_back();
    return pass;
}

int main(){

    vector<vector<float> > flow, border;
    for(int i = 0; i < 5; ++i){
        vector<float> t1,t2;
        for(int i = 0; i < 5; ++i){
            t1.push_back(FLT_MAX);
            t2.push_back(FLT_MAX);
        }
        flow.push_back(t1);
        border.push_back(t2);
        flow[i][i] = 1;
    }
    flow[0][1] = 0;
    flow[0][2] = 0;
    flow[1][2] = 0;
    flow[2][1] = 0;
    flow[1][3] = 0;
    flow[2][3] = 0;
    flow[1][4] = 0;
    flow[3][4] = 0;

    border[0][1] = 4;
    border[0][2] = 2;
    border[1][2] = 2;
    border[2][1] = 1;
    border[1][3] = 1;
    border[2][3] = 3;
    border[1][4] = 1;
    border[3][4] = 5;

    while(true){
        vector<vector<int> > V;
        vector<int> Vk;
        Vk.push_back(0);
        V.push_back(Vk);
        int i= 0;
        bool end = false;
        while(!end){
            Vk.clear();
            for(int k = 0; k < V[i].size(); k++){
                for(int j = 0; j<5; j++){
                    if(j == V[i][k]) continue;
                    if(!pathHasNode(V,j) && !pathHasNode(Vk,j) && (flow[V[i][k]][j] < border[V[i][k]][j] || (flow[j][V[i][k]]>0 && flow[j][V[i][k]] < FLT_MAX))){
                        Vk.push_back(j);
                        if(j == 4){
                            end = true;
                            break;
                        }
                    }
                }
            }
            if(Vk.size()==0) break;
            V.push_back(Vk);
            i++;
        }
        if(!end) break;
        vector<int> path;
        getPath(&flow,&border,&path,0,4);

        float min_c = FLT_MAX;
        for(int i = 0; i < path.size()-1; i++){
            int d = border[path[i]][path[i+1]] - flow[path[i]][path[i+1]];
            if(min_c> d) min_c = d;
        }
        for(int i = 0; i < path.size()-1; i++){
            flow[path[i]][path[i+1]]+= min_c;
        }
    }


    return 0;
}


